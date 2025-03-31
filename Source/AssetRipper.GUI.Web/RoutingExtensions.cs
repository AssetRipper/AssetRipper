using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AssetRipper.GUI.Web;

internal static class RoutingExtensions
{
	internal static RouteHandlerBuilder MapGet(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, RequestDelegate requestDelegate)
	{
		// RouteHandlerBuilder is always returned from this method.
		// We cast it, so we can access Produces<T> and similar methods.
		RouteHandlerBuilder mapped = (RouteHandlerBuilder)EndpointRouteBuilderExtensions.MapGet(endpoints, pattern, requestDelegate);

		// We need to add MethodInfo to the metadata, so that it will be used in the api explorer.
		// https://github.com/dotnet/aspnetcore/issues/44005#issuecomment-1248717069
		// https://github.com/dotnet/aspnetcore/issues/44970
		return mapped.WithMetadata(requestDelegate.Method);
	}

	internal static RouteHandlerBuilder MapPost(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, RequestDelegate requestDelegate)
	{
		// RouteHandlerBuilder is always returned from this method.
		// We cast it, so we can access Produces<T> and similar methods.
		RouteHandlerBuilder mapped = (RouteHandlerBuilder)EndpointRouteBuilderExtensions.MapPost(endpoints, pattern, requestDelegate);

		// We need to add MethodInfo to the metadata, so that it will be used in the api explorer.
		// https://github.com/dotnet/aspnetcore/issues/44005#issuecomment-1248717069
		// https://github.com/dotnet/aspnetcore/issues/44970
		return mapped.WithMetadata(requestDelegate.Method);
	}

	internal static RouteHandlerBuilder MapGet(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, Func<IResult> handler)
	{
		return endpoints.MapGet(pattern, (context) =>
		{
			IResult result = handler.Invoke();
			return result.ExecuteAsync(context);
		});
	}

	internal static RouteHandlerBuilder MapStaticFile(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string path, string contentType)
	{
		return endpoints.MapGet(path, async (context) =>
		{
			string fileName = Path.GetFileName(path);
			byte[] data = await StaticContentLoader.LoadEmbedded(path);
			await Results.Bytes(data, contentType, fileName).ExecuteAsync(context);
		}).Produces<byte[]>(StatusCodes.Status200OK, contentType);
	}

	internal static RouteHandlerBuilder MapRemoteFile(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string path, string contentType, string source, string? integrity = null)
	{
		return endpoints.MapGet(path, async (context) =>
		{
			try
			{
				string fileName = Path.GetFileName(path);
				byte[] data = await StaticContentLoader.LoadRemote(path, source, integrity);
				if (data.Length == 0)
				{
					await Results.NotFound().ExecuteAsync(context);
				}
				else
				{
					await Results.Bytes(data, contentType, fileName).ExecuteAsync(context);
				}
			}
			catch (Exception ex)
			{
				await Results.InternalServerError(ex.ToString()).ExecuteAsync(context);
			}
		}).Produces<byte[]>(StatusCodes.Status200OK, contentType);
	}
}
