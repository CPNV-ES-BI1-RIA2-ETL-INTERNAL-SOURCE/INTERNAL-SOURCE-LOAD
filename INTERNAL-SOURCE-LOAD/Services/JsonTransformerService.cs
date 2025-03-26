using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public class JsonTransformerService : IJsonTransformerService
    {
        private readonly IServiceProvider _serviceProvider;

        public JsonTransformerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public object TransformJsonToModel(JsonElement jsonData, string modelTypeName)
        {
            if (jsonData.ValueKind == JsonValueKind.Undefined || jsonData.ValueKind == JsonValueKind.Null)
            {
                throw new ArgumentException("JSON data cannot be null or undefined.", nameof(jsonData));
            }

            if (string.IsNullOrWhiteSpace(modelTypeName))
            {
                throw new ArgumentException("Model type name cannot be null or empty.", nameof(modelTypeName));
            }

            var targetType = ResolveModelType(modelTypeName);
            if (targetType == null)
            {
                throw new TypeLoadException($"Model type '{modelTypeName}' not found.");
            }

            var transformerType = typeof(IJsonToModelTransformer<>).MakeGenericType(targetType);
            dynamic transformer = _serviceProvider.GetService(transformerType);
            if (transformer == null)
            {
                throw new InvalidOperationException($"No transformer registered for model type: {modelTypeName}");
            }

            try
            {
                var model = transformer.Transform(jsonData);
                if (model == null)
                {
                    throw new InvalidOperationException("Transformation resulted in a null model.");
                }

                return model;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error deserializing JSON to {modelTypeName}: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw; // rethrow specific exceptions
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error transforming JSON to model {modelTypeName}: {ex.Message}", ex);
            }
        }

        public Type ResolveModelType(string modelTypeName)
        {
            if (string.IsNullOrWhiteSpace(modelTypeName))
            {
                throw new ArgumentException("Model type name cannot be null or empty.", nameof(modelTypeName));
            }

            var type = Type.GetType(modelTypeName);
            if (type == null)
            {
                throw new TypeLoadException($"Type '{modelTypeName}' not found.");
            }

            return type;
        }
    }
}