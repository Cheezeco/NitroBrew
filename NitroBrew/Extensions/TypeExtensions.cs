using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NitroBrew.Extensions
{
    internal static class TypeExtensions
    {
        internal static bool IsEnumerableType(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        internal static bool IsListType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
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

        internal static bool CanConvertTo<T>(this object value) where T : class
        {
            return value.CanConvertTo(typeof(T));
        }

        internal static bool CanConvertTo(this object value, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);

            return converter.CanConvertFrom(type);
        }

        internal static T ConvertTo<T>(this object value) where T : class
        {
            var convertedValue = value.ConvertTo(typeof(T));

            return convertedValue is null ? null : (T)convertedValue;
        }

        internal static object ConvertTo(this object value, Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);

            if (!converter.CanConvertFrom(value.GetType())) return null;

            try
            {
                return converter.ConvertFrom(value);
            }
            catch
            {
                return null;
            }
        }
    }
}