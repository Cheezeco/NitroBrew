using System;

namespace NitroBrew.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BridgeTableProcAttribute : Attribute
    {
        public string StoredProcedure { get; set; }

        public BridgeTableProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
