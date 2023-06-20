using NitroBrew.Attributes;
using NitroBrew.Attributes.StoredProcActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NitroBrew
{
    public class IncludeBuilder<T>
    {
        private readonly List<IncludeProperty> _includes;

        public IncludeBuilder()
        {
            _includes = new List<IncludeProperty>();
        }

        public IncludeBuilder<T> Include<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (!(expression.Body is MemberExpression)) return this;

            var type = typeof(T);
            var propType = typeof(TProperty);

            if (propType.IsPrimitive && !typeof(IEnumerable).IsAssignableFrom(propType)) return this;

            var propInfo = type.GetProperties().FirstOrDefault(x => x.PropertyType == propType);
            var relationship = Relationship.OneToOne;
            var storedProc = string.Empty;
            var keyParameterName = string.Empty;
            var isCustom = false;

            if (typeof(IEnumerable).IsAssignableFrom(propType))
            {
                var isManyToMany = propType.GenericTypeArguments[0].GetProperties().Any(x => typeof(IEnumerable).IsAssignableFrom(x.PropertyType)
                                                                     && x.PropertyType.GenericTypeArguments.Length > 0
                                                                     && x.PropertyType.GenericTypeArguments[0].Name == type.Name);

                keyParameterName = type.GetProperties()
                           .FirstOrDefault(x => x.GetCustomAttribute<EntityKeyAttribute>() != null).Name;
                if (isManyToMany)
                {
                    relationship = Relationship.ManyToMany;
                    storedProc = type.GetProperties().FirstOrDefault(x => x.PropertyType == propType).GetCustomAttribute<BridgeTableProcAttribute>().StoredProcedure;

                }
                else
                {
                    relationship = Relationship.OneToMany;
                    storedProc = type.GetProperties().FirstOrDefault(x => x.PropertyType == propType).GetCustomAttribute<OneToManyProcAttribute>().StoredProcedure;
                }
            }
            else
            {
                var customIncludeAttribute = type.GetProperties().FirstOrDefault(x => x.PropertyType == propType).GetCustomAttribute<CustomIncludeProcAttribute>();
                if (customIncludeAttribute != null)
                {
                    storedProc = customIncludeAttribute.StoredProcedure;
                    keyParameterName = type.GetProperties().FirstOrDefault(x => x.GetCustomAttribute<EntityKeyAttribute>() != null).Name;
                    isCustom = true;
                }
                else
                {
                    storedProc = propType.GetCustomAttribute<GetStoredProcAttribute>().StoredProcedure;
                    keyParameterName = propType
                        .GetProperties()
                        .FirstOrDefault(x => x.GetCustomAttribute<EntityKeyAttribute>() != null)
                        .Name;
                }
            }

            _includes.Add(new IncludeProperty()
            {
                KeyParameterName = keyParameterName,
                StoredProcedure = storedProc,
                PropertyInfo = propInfo,
                Relationship = relationship,
                IsCustom = isCustom
            });

            return this;
        }

        internal List<IncludeProperty> Build()
        {
            return _includes;
        }
    }
}
