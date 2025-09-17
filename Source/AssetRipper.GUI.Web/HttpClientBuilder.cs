using System.Net.Http;
using static AssetRipper.Import.AssetRipperRuntimeInformation;

namespace AssetRipper.GUI.Web;

internal static class HttpClientBuilder
{
	internal static HttpClient CreateHttpClient()
	{
		string productName = GameFileLoader.Premium ? "AssetRipper.GUI.Premium" : "AssetRipper.GUI.Free";

		HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.Add(new(productName, Build.Version));
		client.DefaultRequestHeaders.UserAgent.Add(new($"({Build.Configuration}; {ProcessArchitecture}; {Build.Type})"));
		client.DefaultRequestHeaders.UserAgent.Add(new($"({OS.Name}; {OS.Version}; {RamQuantity})"));
		client.DefaultRequestHeaders.UserAgent.Add(new($"({CompileTime})"));
		return client;
	}
}
