using INTERNAL_SOURCE_LOAD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace INTERNAL_SOURCE_LOAD.Controllers;

[Route("api/v1/documents/load")]
[ApiController]
public class LoadController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSettings _appSettings;
    private readonly IDatabaseExecutor _sqlExecutor;

    public LoadController(IServiceProvider serviceProvider, IOptions<AppSettings> appSettings, IDatabaseExecutor sqlExecutor)
    {
        _serviceProvider = serviceProvider;
        _appSettings = appSettings.Value;
        _sqlExecutor = sqlExecutor;
    }

    private bool IsDuplicateKeyError(Exception ex)
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

    [HttpPost]
    public IActionResult Post([FromBody] JsonElement jsonData)
    {
        if (jsonData.ValueKind == JsonValueKind.Undefined || jsonData.ValueKind == JsonValueKind.Null)
        {
            return BadRequest("Invalid JSON payload.");
        }

        try
        {
            // Resolve the target type from the configuration
            var targetType = Type.GetType(_appSettings.DefaultModel);
            if (targetType == null)
            {
                return BadRequest($"Model type '{_appSettings.DefaultModel}' not found.");
            }

            var transformerType = typeof(IJsonToModelTransformer<>).MakeGenericType(targetType);
            dynamic transformer = _serviceProvider.GetService(transformerType);
            if (transformer == null)
            {
                return BadRequest($"No transformer found for model type: {_appSettings.DefaultModel}");
            }

            // Transform JSON into the specified model type
            var model = transformer.Transform(jsonData);

            if (model == null)
            {
                return BadRequest("Transformation resulted in a null model.");
            }

            // Generate SQL queries
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
                catch (Exception ex)
                {
                    if (IsDuplicateKeyError(ex))
                    {
                        // Just skip this record and continue with others
                        skippedDuplicates.Add(ex.Message);
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
                catch (Exception ex)
                {
                    if (IsDuplicateKeyError(ex))
                    {
                        // Skip duplicate foreign key updates
                        skippedDuplicates.Add(ex.Message);
                        continue;
                    }
                    throw;
                }
            }

            var message = $"Data processed for table: {tableName}";
            if (skippedDuplicates.Any())
            {
                message += $". Skipped {skippedDuplicates.Count} duplicate entries.";
            }

            return Ok(message);
        }
        catch (Exception ex)
        {
            if (IsDuplicateKeyError(ex))
            {
                // Even at this level, just report it as a success with skipped items
                return Ok($"Data processed, some duplicate entries were skipped: {ex.Message}");
            }
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing data: {ex.Message}");
        }
    }
}
