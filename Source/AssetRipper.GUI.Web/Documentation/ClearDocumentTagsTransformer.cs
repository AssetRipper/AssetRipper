using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace AssetRipper.GUI.Web.Documentation;

internal class ClearDocumentTagsTransformer : IOpenApiDocumentTransformer
{
	public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
	{
		document.Tags?.Clear();
		return Task.CompletedTask;
	}
}
