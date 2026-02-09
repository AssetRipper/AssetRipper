using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AssetRipper.GUI.Web.Documentation;

internal class InsertionOperationTransformer : IOpenApiOperationTransformer
{
	public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
	{
		foreach (InsertionMetadata<OpenApiParameter> metadata in context.Description.ActionDescriptor.EndpointMetadata.OfType<InsertionMetadata<OpenApiParameter>>())
		{
			operation.Parameters ??= [];
			operation.Parameters.Add(metadata.Value);
		}
		return Task.CompletedTask;
	}
}
