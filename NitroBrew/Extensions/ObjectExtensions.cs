using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NitroBrew.Extensions
{
    public static class ObjectExtensions
    {
        public static bool ComparePrimitiveProperties(this object value, object other)
        {
            if (value.GetType() != other.GetType()) return false;

            var type = value.GetType();

            foreach (var prop in type.GetProperties())
            {
                if (!prop.PropertyType.IsPrimitiveType() || prop.GetValue(value) == prop.GetValue(other)) continue;

                return false;
            }

            return true;
        }

        public static object CreateCopy(this object value)
        {
            if (value.GetType().IsEnumerableType())
            {
                return value.CreateCopyIEnumerable();
            }

            return value.CreateCopyObj();
        }

        private static object CreateCopyIEnumerable(this object value)
        {
            var type = value.GetType();
            var original = value as IEnumerable<object>;
            var copy = (Activator.CreateInstance(type) as IEnumerable<object>).ToList();

            foreach (var item in original)
            {
                copy.Add(item.CreateCopy());
            }

            return copy;
        }

        private static object CreateCopyObj(this object value)
        {
            var type = value.GetType();
            var copy = Activator.CreateInstance(type);

            foreach (var property in type.GetProperties())
            {
                if (!property.PropertyType.IsPrimitiveType() && !property.PropertyType.IsEnum) continue;

                var propertyValue = property.GetValue(value);

                property.SetValue(copy, propertyValue);
            }

            return copy;
        }

        public static bool IsNotNull(this object value)
        {
            return !(value is null);
        }
    }
}