using INTERNAL_SOURCE_LOAD.Models;
using System.Collections;
using System.Reflection;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public static class SqlInsertGenerator
    {
        public static List<(string Query, object Model)> GenerateInsertQueries(string tableName, object data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var queries = new List<(string Query, object Model)>();
            GenerateInsertQueriesRecursive(tableName, data, queries);
            return queries;
        }

        private static void GenerateInsertQueriesRecursive(string tableName, object data, List<(string Query, object Model)> queries)
        {
            if (data == null) return;

            var type = data.GetType();
            var dynamicTableName = GetTableName(type);

            // Handle collections at the top level
            if (IsCollection(type))
            {
                throw new InvalidOperationException("Top-level collections are not supported for insert queries.");
            }

            // Handle complex objects
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = new List<string>();
            var values = new List<string>();

            foreach (var property in properties)
            {
                var value = property.GetValue(data);

                if (value != null)
                {
                    if (IsCollection(property.PropertyType))
                    {
                        var elementType = property.PropertyType.GetGenericArguments().FirstOrDefault();

                        if (elementType != null && IsSimpleType(elementType))
                        {
                            // Flatten collection of simple types into a single string
                            var flattenedValue = string.Join(",", ((IEnumerable)value).Cast<object>());
                            columns.Add(property.Name);
                            values.Add(ConvertToSqlValue(flattenedValue));
                        }
                        else
                        {
                            // Recurse for collections of complex objects
                            foreach (var item in (IEnumerable)value)
                            {
                                GenerateInsertQueriesRecursive(GetTableName(item.GetType()), item, queries);
                            }
                        }
                    }
                    else if (IsSimpleType(property.PropertyType))
                    {
                        // Add simple property
                        columns.Add(property.Name);
                        values.Add(ConvertToSqlValue(value));
                    }
                    else
                    {
                        // Recurse into nested objects
                        GenerateInsertQueriesRecursive(GetTableName(property.PropertyType), value, queries);
                    }
                }
            }

            // Generate query for the current object
            if (columns.Count > 0 && values.Count > 0)
            {
                var query = GenerateInsertQuery(dynamicTableName, columns, values);
                queries.Add((query, data));
            }
        }


        private static string GenerateInsertQuery(string tableName, List<string> columns, List<string> values)
        {
            var columnsPart = string.Join(", ", columns);
            var valuesPart = string.Join(", ", values);

            return $"INSERT INTO {tableName} ({columnsPart}) VALUES ({valuesPart});";
        }



        public static string GetTableName(Type type)
        {
            var attribute = type.GetCustomAttribute<TableNameAttribute>();
            return attribute?.TableName ?? type.Name; // Default to type name if no attribute is found
        }

        private static string ConvertToSqlValue(object value)
        {
            return value switch
            {
                string str => $"'{str.Replace("'", "''")}'",
                DateTime dateTime => $"'{dateTime:yyyy-MM-dd HH:mm:ss}'",
                null => "NULL",
                _ => value.ToString()
            };
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid);
        }

        private static bool IsCollection(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }
        // Function to update foreign keys dynamically
        public static List<string> GenerateUpdateForeignKeysQueries(object model, Dictionary<object, long> modelIds)
        {
            var queries = new List<string>(); // To store generated queries
            var modelType = model.GetType();

            foreach (var property in modelType.GetProperties())
            {
                // Identify FK properties (e.g., properties ending with 'ID')
                if (property.Name.EndsWith("ID"))
                {
                    // Infer the related model name by removing "ID" from the property name
                    string relatedModelName = property.Name.Replace("ID", "");

                    // Find the related model in the dictionary based on its type name
                    var relatedModel = modelIds.Keys.FirstOrDefault(m => m.GetType().Name == relatedModelName);

                    if (relatedModel != null)
                    {
                        // Get the ID of the related model
                        var relatedModelId = modelIds[relatedModel];

                        // Use a utility function to get the table name
                        var tableName = SqlInsertGenerator.GetTableName(modelType);

                        // Generate the update query
                        string updateQuery = $"UPDATE {tableName} SET {property.Name} = {relatedModelId} WHERE Id = {modelIds[model]}";

                        // Add the generated query to the list
                        queries.Add(updateQuery);
                    }
                }
            }

            return queries; // Return the list of queries
        }
    }

}
