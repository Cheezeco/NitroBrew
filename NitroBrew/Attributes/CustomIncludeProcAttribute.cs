using System;
using System.Collections.Generic;
using System.Text;

namespace NitroBrew.Attributes
{
    public class CustomIncludeProcAttribute : Attribute
    {
        public string StoredProcedure { get; set; }

        public CustomIncludeProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
