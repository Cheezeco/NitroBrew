using System.Reflection;

namespace NitroBrew
{
    internal class IncludeProperty
    {
        public int Id { get; set; }
        public string KeyParameterName { get; set; }
        public string StoredProcedure { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public Relationship Relationship { get; set; }
        public bool IsCustom { get; set; }
    }
}
