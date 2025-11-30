using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AssetRipper.GUI.Web.Documentation;

internal class ClearOperationTagsTransformer : IOpenApiOperationTransformer
{
	public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
	{
		operation.Tags?.Clear();
		return Task.CompletedTask;
	}
}
