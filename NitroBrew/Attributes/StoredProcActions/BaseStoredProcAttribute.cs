using System;

namespace NitroBrew.Attributes.StoredProcActions
{
    public abstract class BaseStoredProcAttribute : Attribute
    {
        public string StoredProcedure { get; set; }
    }
}
