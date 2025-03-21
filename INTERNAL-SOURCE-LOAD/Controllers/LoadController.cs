using INTERNAL_SOURCE_LOAD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Text.Json;

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
            foreach (var query in sqlQueries)
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
            List<string> sqlQueriesFK = new List<string>();
            foreach (var modelInstance in modelIds.Keys)
            {
                sqlQueriesFK.AddRange(SqlInsertGenerator.GenerateUpdateForeignKeysQueries(modelInstance, modelIds));
            }

            foreach (var query in sqlQueriesFK)
            {
                _sqlExecutor.Execute(query);
            }


            return StatusCode(StatusCodes.Status201Created, $"Data inserted into table: {tableName}");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing data: {ex.Message}");
        }
    }
    
}
