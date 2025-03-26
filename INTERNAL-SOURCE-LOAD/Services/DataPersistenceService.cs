using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using INTERNAL_SOURCE_LOAD.Models.DTOs;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public class DataPersistenceService : IDataPersistenceService
    {
        private readonly IDatabaseExecutor _sqlExecutor;

        public DataPersistenceService(IDatabaseExecutor sqlExecutor)
        {
            _sqlExecutor = sqlExecutor;
        }

        public PersistenceResult PersistData(object model, string modelTypeName)
        {
            var result = new PersistenceResult
            {
                Success = true,
                SkippedDuplicates = 0,
                Message = ""
            };

            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                // Get model type
                var targetType = Type.GetType(modelTypeName);
                if (targetType == null)
                {
                    throw new ArgumentException($"Model type '{modelTypeName}' not found.");
                }

                string tableName = targetType.Name;
                var sqlQueries = SqlInsertGenerator.GenerateInsertQueries(tableName, model);

                // Step 3: Execute insert queries and track inserted IDs
                var modelIds = new Dictionary<object, long>();
                var skippedDuplicates = new List<string>();

                foreach (var query in sqlQueries)
                {
                    try
                    {
                        // Execute the insert query and get the inserted ID
                        var insertedId = _sqlExecutor.ExecuteAndReturnId(query.Item1);

                        // Map the model instance to the ID
                        var modelInstance = query.Item2;
                        if (modelInstance != null)
                        {
                            modelIds[modelInstance] = insertedId;
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062)
                    {
                        // Just skip this record and continue with others
                        skippedDuplicates.Add(ex.Message);
                        result.SkippedDuplicates++;
                        continue;
                    }
                    catch (DbException ex) when (ex.Message.Contains("Duplicate entry"))
                    {
                        // Handle other database providers with duplicate entries
                        skippedDuplicates.Add(ex.Message);
                        result.SkippedDuplicates++;
                        continue;
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Database operation failed"))
                    {
                        // Handle database operation failures from MariaDbExecutor
                        result.Success = false;
                        result.Message = ex.Message;
                        result.Errors.Add(ex.Message);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Check if it's a wrapped DB exception for testing scenarios
                        if (IsDuplicateKeyError(ex))
                        {
                            skippedDuplicates.Add(ex.Message);
                            result.SkippedDuplicates++;
                            continue;
                        }
                        throw;
                    }
                }

                List<string> sqlQueriesFK = new List<string>();
                foreach (var modelInstance in modelIds.Keys)
                {
                    sqlQueriesFK.AddRange(SqlInsertGenerator.GenerateUpdateForeignKeysQueries(modelInstance, modelIds));
                }

                foreach (var query in sqlQueriesFK)
                {
                    try
                    {
                        _sqlExecutor.Execute(query);
                    }
                    catch (MySqlException ex) when (ex.Number == 1062)
                    {
                        // Skip duplicate foreign key updates
                        skippedDuplicates.Add(ex.Message);
                        result.SkippedDuplicates++;
                        continue;
                    }
                    catch (DbException ex) when (ex.Message.Contains("Duplicate entry"))
                    {
                        // Handle other database providers with duplicate entries
                        skippedDuplicates.Add(ex.Message);
                        result.SkippedDuplicates++;
                        continue;
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Database operation failed"))
                    {
                        // Handle database operation failures from MariaDbExecutor
                        result.Success = false;
                        result.Message = ex.Message;
                        result.Errors.Add(ex.Message);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Check if it's a wrapped DB exception for testing scenarios
                        if (IsDuplicateKeyError(ex))
                        {
                            skippedDuplicates.Add(ex.Message);
                            result.SkippedDuplicates++;
                            continue;
                        }
                        throw;
                    }
                }

                result.Message = $"Data processed for table: {tableName}";
                if (skippedDuplicates.Any())
                {
                    result.Message += $". Skipped {skippedDuplicates.Count} duplicate entries.";
                }

                return result;
            }
            catch (ArgumentNullException ex)
            {
                result.Success = false;
                result.Message = $"Invalid input: {ex.Message}";
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                result.Success = false;
                result.Message = $"Invalid argument: {ex.Message}";
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                // Even at this level, just report it as a success with skipped items
                result.Message = $"Data processed, some duplicate entries were skipped: {ex.Message}";
                result.SkippedDuplicates++;
                return result;
            }
            catch (DbException ex) when (ex.Message.Contains("Duplicate entry"))
            {
                // Handle other database providers with duplicate entries
                result.Message = $"Data processed, some duplicate entries were skipped: {ex.Message}";
                result.SkippedDuplicates++;
                return result;
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Database operation") ||
                                                      ex.Message.StartsWith("Failed to get last inserted ID") ||
                                                      ex.Message.StartsWith("Failed to convert database ID"))
            {
                result.Success = false;
                result.Message = $"Database error: {ex.Message}";
                result.Errors.Add(ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                // Keep this as a last resort for any other exceptions
                if (IsDuplicateKeyError(ex))
                {
                    result.Message = $"Data processed, some duplicate entries were skipped: {ex.Message}";
                    result.SkippedDuplicates++;
                    return result;
                }

                result.Success = false;
                result.Message = $"Error processing data: {ex.Message}";
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public bool IsDuplicateKeyError(Exception ex)
        {
            // Check if it's a direct MySqlException
            if (ex is MySqlException mysqlEx && mysqlEx.Number == 1062)
                return true;

            // Check if it's our test wrapper exception
            if (ex.Message?.Contains("Duplicate entry") == true ||
                ex.InnerException?.Message?.Contains("MySQL Error Code: 1062") == true)
                return true;

            return false;
        }
    }
}