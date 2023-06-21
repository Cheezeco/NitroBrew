using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NitroBrew.Extensions
{
    internal static class TypeExtensions
    {
        internal static bool IsEnumerableType(this Type type)
        {
            return typeof(IEnumerable<>).IsAssignableFrom(type);
        }

        internal static bool IsPrimitiveType(this Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
        }

        internal static PropertyInfo GetEnumerableGenericArgument(this Type type, Type typeToFind)
        {
            if (!IsEnumerableType(type) || type.GenericTypeArguments.Length != 1)
                return null;

            return type.GetProperties().GetPropertyWithMatchingGenericType(typeToFind);
        }
    }
}