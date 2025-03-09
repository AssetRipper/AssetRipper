using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace AssetRipper.GUI.Web.Documentation;

internal class SortDocumentPathsTransformer : IOpenApiDocumentTransformer
{
	public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
	{
		OpenApiPaths newPaths = new();
		newPaths.Extensions = document.Paths.Extensions;
		foreach ((string key, OpenApiPathItem value) in document.Paths.OrderBy(pair => pair.Key))
		{
			newPaths.Add(key, value);
		}
		document.Paths = newPaths;
		return Task.CompletedTask;
	}
}
