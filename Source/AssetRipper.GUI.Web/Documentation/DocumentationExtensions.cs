using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;

namespace AssetRipper.GUI.Web.Documentation;

internal static class DocumentationExtensions
{
	public static RouteHandlerBuilder WithParameter(this RouteHandlerBuilder builder, OpenApiParameter parameter)
	{
		return builder.WithMetadata(new InsertionMetadata<OpenApiParameter>(parameter));
	}

	public static RouteHandlerBuilder WithQueryStringParameter(this RouteHandlerBuilder builder, string name, string description = "", bool required = false)
	{
		return builder.WithParameter(new OpenApiParameter
		{
			Name = name,
			In = ParameterLocation.Query,
			Description = description,
			Required = required,
			Schema = new OpenApiSchema
			{
				Type = JsonSchemaType.String,
			},
		});
	}

	public static RouteHandlerBuilder ProducesHtmlPage(this RouteHandlerBuilder builder)
	{
		return builder.Produces<string>(StatusCodes.Status200OK, "text/html");
	}
}
