using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace INTERNAL_SOURCE_LOAD.Services
{
    public class JsonTransformerService : IJsonTransformerService
    {
        private readonly IServiceProvider _serviceProvider;

        public JsonTransformerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object TransformJsonToModel(JsonElement jsonData, string modelTypeName)
        {
            var targetType = ResolveModelType(modelTypeName);
            if (targetType == null)
            {
                throw new ArgumentException($"Model type '{modelTypeName}' not found.");
            }

            var transformerType = typeof(IJsonToModelTransformer<>).MakeGenericType(targetType);
            dynamic transformer = _serviceProvider.GetService(transformerType);
            if (transformer == null)
            {
                throw new InvalidOperationException($"No transformer found for model type: {modelTypeName}");
            }

            var model = transformer.Transform(jsonData);
            if (model == null)
            {
                throw new InvalidOperationException("Transformation resulted in a null model.");
            }

            return model;
        }

        public Type ResolveModelType(string modelTypeName)
        {
            return Type.GetType(modelTypeName);
        }
    }
}