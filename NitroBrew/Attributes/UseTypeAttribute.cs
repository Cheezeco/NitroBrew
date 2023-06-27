using System;
using System.Collections.Generic;
using System.Text;

namespace NitroBrew.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UseTypeAttribute : Attribute
    {
        public Type TypeToUse { get; set; }

        public UseTypeAttribute(Type type)
        {
            TypeToUse = type;
        }

    }
}
