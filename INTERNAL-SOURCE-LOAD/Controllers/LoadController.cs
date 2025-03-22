using INTERNAL_SOURCE_LOAD.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace INTERNAL_SOURCE_LOAD.Controllers;

[Route("api/v1/documents/load")]
[ApiController]
public class LoadController : ControllerBase
{
    private readonly IJsonTransformerService _jsonTransformerService;
    private readonly IDataPersistenceService _dataPersistenceService;
    private readonly AppSettings _appSettings;

    public LoadController(
        IJsonTransformerService jsonTransformerService,
        IDataPersistenceService dataPersistenceService,
        IOptions<AppSettings> appSettings)
    {
        _jsonTransformerService = jsonTransformerService;
        _dataPersistenceService = dataPersistenceService;
        _appSettings = appSettings.Value;
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
            // Transform JSON to model
            var model = _jsonTransformerService.TransformJsonToModel(jsonData, _appSettings.DefaultModel);

            // Persist the data
            var result = _dataPersistenceService.PersistData(model, _appSettings.DefaultModel);

            if (result.Success)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing data: {ex.Message}");
        }
    }
}
