using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace AspNetExample.NSwag;

public class OnlyApiOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        return context.OperationDescription.Path
            .StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }
}