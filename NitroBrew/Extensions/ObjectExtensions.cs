using System;

namespace NitroBrew.Extensions
{
    public static class ObjectExtensions
    {
        public static object CreateCopy(this object value)
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