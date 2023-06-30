using System;
using System.Collections.Generic;
using System.Text;

namespace NitroBrew.Attributes
{
    public class CustomIncludeProcAttribute : Attribute
    {
        public string StoredProcedure { get; set; }
        public bool UseEntityKey { get; set; } = true;

        public CustomIncludeProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
