using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages;

public interface ICommand
{
	static abstract Task Start(HttpRequest request);
	static virtual string RedirectionTarget => "/";
}
