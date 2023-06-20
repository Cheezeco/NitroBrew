namespace NitroBrew.Attributes.StoredProcActions
{
    public class GetStoredProcAttribute : BaseStoredProcAttribute
    {
        public GetStoredProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
