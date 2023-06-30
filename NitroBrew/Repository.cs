using Dapper;
using Microsoft.Data.SqlClient;
using NitroBrew.Attributes;
using NitroBrew.Attributes.StoredProcActions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NitroBrew.Extensions;

namespace NitroBrew
{
    public class Repository
    {
        private readonly string _connectionString;
        private readonly Cache _cache;

        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Repository(string connectionString, Cache cache)
        {
            _connectionString = connectionString;
            _cache = cache;
        }

        public void LoadIncludes<T>(IEnumerable<T> entities, IncludeBuilder<T> includeBuilder) where T : class
        {
            if (includeBuilder is null) return;

            using (var connection = GetDbConnection())
            {
                connection.Open();

                foreach (var entity in entities)
                {
                    LoadIncludes(entity, includeBuilder, connection);
                }

                connection.Close();
            }
        }

        public void LoadIncludes<T>(T entity, IncludeBuilder<T> includeBuilder) where T : class
        {
            if (includeBuilder is null) return;

            using (var connection = GetDbConnection())
            {
                connection.Open();

                LoadIncludes(entity, includeBuilder, connection);

                connection.Close();
            }
        }

        private void LoadIncludes<T>(T entity, IncludeBuilder<T> includeBuilder, IDbConnection connection)
            where T : class
        {
            if (includeBuilder is null) return;

            var getMethod = FindPrivateMethod("Get");
            var getManyMethod = FindPrivateMethod("GetMany");

            var includes = GetPreparedIncludes(entity, includeBuilder);

            foreach (var prop in includes)
            {
                MethodInfo method;
                Type type;

                if (prop.Relationship != Relationship.OneToOne)
                {
                    method = getManyMethod;
                    type = prop.PropertyInfo.PropertyType.GenericTypeArguments[0];
                }
                else
                {
                    method = getMethod;
                    type = prop.PropertyInfo.PropertyType;
                }

                var value = InvokeGet(type, method, prop.StoredProcedure, prop.KeyParameterName, prop.Id,
                    connection);

                var propType = prop.PropertyInfo.PropertyType;
                var valueType = value.GetType();

                if (propType.IsListType() && !valueType.IsListType() && valueType.IsEnumerableType())
                {
                    var val = (IEnumerable<object>)value;

                    var castMethod = typeof(Enumerable)
                        .GetMethod("Cast").MakeGenericMethod(new[] { type });

                    value = castMethod.Invoke(null, new object[] { val });

                    var toListMethod = typeof(Enumerable)
                        .GetMethod("ToList").MakeGenericMethod(new[] { type });

                    value = toListMethod.Invoke(null, new object[] { value });
                }

                prop.PropertyInfo.SetValue(entity, value);
            }
        }

        public T Get<T>(int id, IncludeBuilder<T> includeBuilder = null) where T : class
        {
            using (var connection = GetDbConnection())
            {
                connection.Open();

                var getMethod = FindPrivateMethod("Get");

                var test = GetMainEntity<T>();

                var entity = InvokeGet<T>(getMethod, test.StoredProcedure, test.KeyParameterName, id, connection);

                if (entity is null)
                {
                    connection.Close();
                    return entity;
                }

                LoadIncludes(entity, includeBuilder, connection);

                connection.Close();

                return entity;
            }
        }

        public IEnumerable<T> GetAll<T>(IncludeBuilder<T> includeBuilder = null) where T : class
        {
            using (var connection = GetDbConnection())
            {
                connection.Open();

                var storedProc = GetStoredProc<T, GetAllStoredProcAttribute>();

                var queryResult = connection.Query(storedProc, commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object>>();
                var entities = Parse<T>(queryResult);

                foreach (var entity in entities)
                {
                    LoadIncludes(entity, includeBuilder, connection);
                    _cache?.Add(GetKey(entity), entity);
                }

                connection.Close();

                return entities;
            }
        }

        public void Update<T>(T entity) where T : class
        {
            var storedProc = GetStoredProc<T, UpdateStoredProcAttribute>();

            using (var connection = GetDbConnection())
            {
                connection.Open();

                connection.ExecuteScalar(storedProc, CreateDynamicParameters(entity, true),
                    commandType: CommandType.StoredProcedure);

                connection.Close();
            }

            _cache?.Add(GetKey(entity), entity);
        }

        public int Insert<T>(T entity) where T : class
        {
            var storedProc = GetStoredProc<T, InsertStoredProcAttribute>();

            using (var connection = GetDbConnection())
            {
                connection.Open();

                var id = connection.ExecuteScalar<int>(storedProc, CreateDynamicParameters(entity),
                    commandType: CommandType.StoredProcedure);

                connection.Close();

                _cache?.Add(id, entity);

                return id;
            }
        }

        public void Delete<T>(int id) where T : class
        {
            var storedProc = GetStoredProc<T, DeleteStoredProcAttribute>();

            using (var connection = GetDbConnection())
            {
                connection.Open();

                var keyParameterName = typeof(T).GetProperties()
                    .FirstOrDefault(x => x.GetCustomAttribute<EntityKeyAttribute>() != null).Name;
                var parameters = new DynamicParameters();
                parameters.Add(keyParameterName, id);

                connection.ExecuteScalar(storedProc, parameters, commandType: CommandType.StoredProcedure);

                connection.Close();
            }

            _cache?.Remove<T>(id);
        }

        private T Get<T>(string storedProc, string keyParameterName, int id, IDbConnection connection) where T : class
        {
            if (_cache.IsNotNull() && _cache.TryGet(id, out T value))
            {
                if (value.IsNotNull()) return value;
            }
            dynamic queryResult;

            if (keyParameterName == "-")
            {
                queryResult = connection.QuerySingleOrDefault(storedProc, commandType: CommandType.StoredProcedure);
            }
            else
            {
                var parameters = new DynamicParameters();
                parameters.Add(keyParameterName, id);

                queryResult = connection.QuerySingleOrDefault(storedProc, parameters, commandType: CommandType.StoredProcedure);
            }


            var parsedValue = Parse<T>(queryResult as IDictionary<string, object>);

            _cache?.Add(id, parsedValue);

            return parsedValue;
        }

        private IEnumerable<T> GetMany<T>(string storedProc, string keyParameterName, int id, IDbConnection connection)
            where T : class
        {
            var value = _cache?.GetEnumerable<T>(id);

            if (value.IsNotNull()) return value;


            dynamic queryResult;

            if (keyParameterName == "-")
            {
                queryResult = connection.Query(storedProc, commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object>>();
            }
            else
            {
                var parameters = new DynamicParameters();
                parameters.Add(keyParameterName, id);

                queryResult = connection.Query(storedProc, parameters, commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object>>();
            }

            var parsedValue = Parse<T>(queryResult);

            _cache?.Add(id, parsedValue);

            return parsedValue;
        }

        #region Helpers

        private IEnumerable<T> Parse<T>(IEnumerable<IDictionary<string, object>> values)
        {
            var items = new List<T>();

            foreach (var value in values)
            {
                items.Add(Parse<T>(value));
            }

            return items;
        }

        private T Parse<T>(IDictionary<string, object> values)
        {
            var entity = Activator.CreateInstance<T>();

            foreach (var prop in typeof(T).GetProperties())
            {
                var ignoreAttribute = prop.GetCustomAttribute<IgnorePropertyAttribute>();
                var columnNameAttribute = prop.GetCustomAttribute<ColumnNameAttribute>();
                var useTypeAttribute = prop.GetCustomAttribute<UseTypeAttribute>();

                if (ignoreAttribute != null) continue;

                var name = columnNameAttribute != null ? columnNameAttribute.Name : prop.Name;

                if (values is null || !values.TryGetValue(name, out var value)) continue;

                if (useTypeAttribute.IsNotNull())
                {
                    value = value.ConvertTo(useTypeAttribute.TypeToUse);
                }

                prop.SetValue(entity, value);
            }

            return entity;
        }

        private T InvokeGet<T>(MethodInfo method, string storedProc, string keyParameterName, int id,
            IDbConnection connection) where T : class
        {
            return method.MakeGenericMethod(new Type[] { typeof(T) })
                .Invoke(this, new object[] { storedProc, keyParameterName, id, connection }) as T;
        }

        private object InvokeGet(Type type, MethodInfo method, string storedProc, string keyParameterName, int id,
            IDbConnection connection)
        {
            return method.MakeGenericMethod(new Type[] { type })
                .Invoke(this, new object[] { storedProc, keyParameterName, id, connection });
        }

        private MethodInfo FindPrivateMethod(string methodName)
        {
            return typeof(Repository).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private DynamicParameters CreateDynamicParameters<T>(T entity, bool includeId = false)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var parameters = new DynamicParameters();

            foreach (var prop in properties)
            {
                if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string)) continue;

                var attributes = prop.GetCustomAttributes();

                if (!includeId && attributes.Any(x =>
                        x.GetType() == typeof(EntityKeyAttribute) || x.GetType() == typeof(IgnorePropertyAttribute)))
                    continue;

                var columnNameAttribute = attributes.FirstOrDefault(x => x.GetType() == typeof(ColumnNameAttribute));
                string columnName = prop.Name;

                if (columnNameAttribute != null)
                {
                    columnName = (columnNameAttribute as ColumnNameAttribute).Name;
                }

                parameters.Add(columnName, prop.GetValue(entity));
            }

            return parameters;
        }

        private IDbConnection GetDbConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private string GetStoredProc<T, TProcedure>()
            where TProcedure : BaseStoredProcAttribute
            where T : class
        {
            return typeof(T).GetCustomAttribute<TProcedure>().StoredProcedure;
        }

        private List<IncludeProperty> GetPreparedIncludes<T>(T entity, IncludeBuilder<T> includeBuilder) where T : class
        {
            var includes = includeBuilder.Build();
            foreach (var include in includes)
            {
                if (include.IsCustom && !include.UseEntityKey)
                {
                    include.Id = -1;
                    continue;
                }

                if (include.Relationship == Relationship.ManyToMany || include.Relationship == Relationship.OneToMany ||
                    include.IsCustom)
                {
                    include.Id = (int)typeof(T).GetProperties()
                        .FirstOrDefault(x => x.GetCustomAttribute<EntityKeyAttribute>() != null)
                        .GetValue(entity);
                }
                else
                {
                    include.Id = (int)typeof(T).GetProperties()
                        .FirstOrDefault(x => x.GetCustomAttribute<IdForEntityAttribute>() != null
                                             && x.GetCustomAttribute<IdForEntityAttribute>().PropertyName ==
                                             include.PropertyInfo.Name)
                        .GetValue(entity);
                }
            }

            return includes;
        }

        private MainEntity GetMainEntity<T>() where T : class
        {
            var mainEntity = new MainEntity();

            var type = typeof(T);
            mainEntity.StoredProcedure = GetStoredProc<T, GetStoredProcAttribute>();
            mainEntity.KeyParameterName = type.GetProperties()
                .FirstOrDefault(x => x.GetCustomAttribute<EntityKeyAttribute>() != null).Name;

            return mainEntity;
        }

        private int? GetKey<T>(T entity) where T : class
        {
            var type = typeof(T);
            var entityKeyProperty =
                type.GetProperties().FirstOrDefault(x => x.GetCustomAttribute<EntityKeyAttribute>().IsNotNull());

            if (entityKeyProperty is null) return -1;

            return entityKeyProperty.GetValue(entity) as int?;
        }

        #endregion
    }
}