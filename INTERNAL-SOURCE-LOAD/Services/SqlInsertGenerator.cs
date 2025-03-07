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
            var queries = new List<string>();
            var modelType = model.GetType();
            var tableName = GetTableName(modelType);
            var processedRelationships = new HashSet<string>(); // Track which foreign keys we've handled

            // First handle navigation properties (e.g., Train)
            foreach (var property in modelType.GetProperties())
            {
                if (IsSimpleType(property.PropertyType))
                    continue;

                var navigationPropertyValue = property.GetValue(model);
                if (navigationPropertyValue == null)
                    continue;

                // Handle direct navigation properties
                if (modelIds.ContainsKey(navigationPropertyValue))
                {
                    var foreignKeyId = modelIds[navigationPropertyValue];
                    var foreignKeyPropertyName = $"{property.Name}ID";

                    // Add to processed list to avoid duplicates
                    processedRelationships.Add(foreignKeyPropertyName);

                    string updateQuery = $"UPDATE {tableName} SET {foreignKeyPropertyName} = {foreignKeyId} WHERE Id = {modelIds[model]}";
                    queries.Add(updateQuery);
                }
            }

            // Then handle explicit ID properties that weren't handled by navigation properties
            foreach (var property in modelType.GetProperties())
            {
                if (!property.Name.EndsWith("ID") || property.Name == "Id")
                    continue;

                // Skip if we already handled this relationship via navigation property
                if (processedRelationships.Contains(property.Name))
                    continue;

                // Find corresponding entity in modelIds
                var relatedEntityName = property.Name.Substring(0, property.Name.Length - 2); // Remove "ID" suffix
                var relatedEntity = modelIds.Keys.FirstOrDefault(k => k.GetType().Name == relatedEntityName);

                if (relatedEntity != null)
                {
                    var foreignKeyId = modelIds[relatedEntity];
                    string updateQuery = $"UPDATE {tableName} SET {property.Name} = {foreignKeyId} WHERE Id = {modelIds[model]}";
                    queries.Add(updateQuery);
                }
            }

            return queries;
        }
    }

}
