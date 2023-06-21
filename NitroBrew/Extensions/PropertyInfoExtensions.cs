using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NitroBrew.Extensions
{
    internal static class PropertyInfoExtensions
    {
        internal static string GetCustomAttributePropertyName<TAttribute>(this IEnumerable<PropertyInfo> propertyInfos)
            where TAttribute : Attribute
        {
            return propertyInfos.FirstOrDefault(x => x.GetCustomAttribute<TAttribute>() != null)?.Name ?? "";
        }

        internal static PropertyInfo GetPropertyWithMatchingGenericType(this IEnumerable<PropertyInfo> properties,
            Type typeToFind)
        {
            return properties.FirstOrDefault(x =>
                x.PropertyType.IsEnumerableType() &&
                x.PropertyType.GenericTypeArguments.Length == 1 &&
                x.PropertyType.GenericTypeArguments[0] == typeToFind);
        }

        internal static PropertyInfo FindProperty(this IEnumerable<PropertyInfo> properties, Type propertyType)
        {
            return properties.FirstOrDefault(x => x.PropertyType == propertyType);
        }

        internal static TAttribute GetCustomAttribute<TAttribute>(this IEnumerable<PropertyInfo> propertyInfos)
            where TAttribute : Attribute
        {
            return propertyInfos
                .Select(x => x.GetCustomAttribute<TAttribute>())
                .FirstOrDefault(attribute => attribute != null);
        }
    }
}