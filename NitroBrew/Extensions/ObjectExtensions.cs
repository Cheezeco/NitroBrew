using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NitroBrew.Extensions
{
    public static class ObjectExtensions
    {
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
                if (!property.PropertyType.IsPrimitiveType()) continue;

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