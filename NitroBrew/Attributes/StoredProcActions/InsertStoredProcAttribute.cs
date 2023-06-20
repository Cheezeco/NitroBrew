namespace NitroBrew.Attributes.StoredProcActions
{
    public class InsertStoredProcAttribute : BaseStoredProcAttribute
    {
        public InsertStoredProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
