using System;

namespace NitroBrew.Attributes
{
    public class IdForEntityAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public IdForEntityAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
