using System;

namespace NitroBrew.Attributes
{
    public class ColumnNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
}
