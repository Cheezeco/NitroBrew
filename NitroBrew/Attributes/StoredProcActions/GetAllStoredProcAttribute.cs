namespace NitroBrew.Attributes.StoredProcActions
{
    public class GetAllStoredProcAttribute : BaseStoredProcAttribute
    {
        public GetAllStoredProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
