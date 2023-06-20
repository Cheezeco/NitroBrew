using System;

namespace NitroBrew.Attributes
{
    public class OneToManyProcAttribute : Attribute
    {
        public string StoredProcedure { get; set; }

        public OneToManyProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
