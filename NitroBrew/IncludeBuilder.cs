using NitroBrew.Attributes;
using NitroBrew.Attributes.StoredProcActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NitroBrew.Extensions;

namespace NitroBrew
{
    public class IncludeBuilder<T> where T : class
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
            var propertyType = typeof(TProperty);

            if (propertyType.IsPrimitiveType() && !propertyType.IsEnumerableType()) return this;

            var typeProperties = type.GetProperties();

            var propertyInfo = typeProperties.FindProperty(propertyType);
            var relationship = Relationship.OneToOne;
            var storedProc = string.Empty;
            var keyParameterName = string.Empty;
            var usesCustomInclude = false;

            if (propertyType.IsEnumerableType())
            {
                var isManyToMany = propertyType.GetEnumerableGenericArgument(type) != null;

                IncludeEnumerableProperty(typeProperties, isManyToMany, out relationship, out storedProc,
                    out keyParameterName);
            }
            else
            {
                IncludeProperty(typeProperties, propertyType, out keyParameterName, out storedProc,
                    ref usesCustomInclude);
            }

            var includeProperty = new IncludeProperty()
            {
                KeyParameterName = keyParameterName,
                StoredProcedure = storedProc,
                PropertyInfo = propertyInfo,
                Relationship = relationship,
                IsCustom = usesCustomInclude
            };

            if (!IsValidIncludeProperty(includeProperty)) return this;

            _includes.Add(includeProperty);

            return this;
        }

        private static void IncludeProperty(PropertyInfo[] typeProperties, Type propertyType,
            out string keyParameterName, out string storedProc, ref bool usesCustomInclude)
        {
            var customIncludeAttribute = typeProperties.GetCustomAttribute<CustomIncludeProcAttribute>();

            if (customIncludeAttribute != null)
            {
                storedProc = customIncludeAttribute.StoredProcedure;
                keyParameterName = typeProperties.GetCustomAttributePropertyName<EntityKeyAttribute>();
                usesCustomInclude = true;
            }
            else
            {
                storedProc = propertyType.GetCustomAttribute<GetStoredProcAttribute>().StoredProcedure;
                keyParameterName = propertyType.GetProperties()
                    .GetCustomAttributePropertyName<EntityKeyAttribute>();
            }
        }

        private static void IncludeEnumerableProperty(PropertyInfo[] typeProperties, bool isManyToMany,
            out Relationship relationship, out string storedProc, out string keyParameterName)
        {
            keyParameterName = typeProperties.GetCustomAttributePropertyName<EntityKeyAttribute>();

            if (isManyToMany)
            {
                relationship = Relationship.ManyToMany;
                storedProc = typeProperties.GetCustomAttribute<BridgeTableProcAttribute>().StoredProcedure ?? "";
            }
            else
            {
                relationship = Relationship.OneToMany;
                storedProc = typeProperties.GetCustomAttribute<OneToManyProcAttribute>().StoredProcedure ?? "";
            }
        }

        private static bool IsValidIncludeProperty(IncludeProperty includeProperty)
        {
            return !string.IsNullOrWhiteSpace(includeProperty.KeyParameterName)
                   && !string.IsNullOrWhiteSpace(includeProperty.StoredProcedure)
                   && includeProperty.PropertyInfo != null;
        }

        internal List<IncludeProperty> Build()
        {
            return _includes;
        }
    }
}