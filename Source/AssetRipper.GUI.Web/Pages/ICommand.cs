using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages;

public interface ICommand
{
	/// <summary>
	/// Execute the command.
	/// </summary>
	/// <param name="request">The Http request.</param>
	/// <returns>The url target for redirection. If null, the website root is used.</returns>
	static abstract Task<string?> Execute(HttpRequest request);
}
