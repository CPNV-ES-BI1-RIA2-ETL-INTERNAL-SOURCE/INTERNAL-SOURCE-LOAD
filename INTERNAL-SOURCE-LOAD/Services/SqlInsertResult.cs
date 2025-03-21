using System.Reflection;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public class SqlInsertResult
    {
        public string Query { get; set; }
        public string TableName { get; set; }
        public object Model { get; set; }
        public List<SqlInsertResult> ChildInserts { get; set; } = new();
        public PropertyInfo ForeignKeyProperty { get; set; }
    }
}
