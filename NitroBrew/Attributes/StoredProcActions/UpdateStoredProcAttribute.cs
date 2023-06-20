namespace NitroBrew.Attributes.StoredProcActions
{
    public class UpdateStoredProcAttribute : BaseStoredProcAttribute
    {
        public UpdateStoredProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
