using Microsoft.AspNetCore.Http;
using System.Text;

namespace AssetRipper.Web.Content;

public abstract class WebContent
{
	public abstract void Write(TextWriter writer);

	public sealed override string ToString()
	{
		StringWriter writer = new()
		{
			NewLine = "\n",
		};
		Write(writer);
		return writer.ToString();
	}

	public Task WriteToResponse(HttpResponse response)
	{
		response.ContentType = ContentType;
		return response.WriteAsync(ToString());
	}

	public IResult ToResult()
	{
		return Results.Text(ToString(), ContentType, Encoding.UTF8);
	}

	/// <summary>
	/// <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types">Mime Type</see>
	/// </summary>
	public abstract string ContentType { get; }
}
