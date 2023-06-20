namespace NitroBrew.Attributes.StoredProcActions
{
    public class DeleteStoredProcAttribute : BaseStoredProcAttribute
    {
        public DeleteStoredProcAttribute(string storedProcedure)
        {
            StoredProcedure = storedProcedure;
        }
    }
}
