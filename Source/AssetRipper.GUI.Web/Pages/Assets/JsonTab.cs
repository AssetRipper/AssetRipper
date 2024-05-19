using AssetRipper.Assets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class JsonTab(IUnityObjectBase asset, HttpClient httpClient) : HtmlTab
{
    private readonly HttpClient httpClient;
    private readonly IUnityObjectBase asset;

    public string FileName { get; } = $"{asset.GetBestName()}.json";
    public override string DisplayName => Localization.Json;
    public override string HtmlName => "json";

    public JsonTab(IUnityObjectBase asset, HttpClient httpClient)
    {
        this.asset = asset;
        this.httpClient = httpClient;
    }

    public override async Task WriteAsync(TextWriter writer)
    {
        var jsonResponse = await httpClient.GetFromJsonAsync<string>($"/Assets/Json?assetPath={asset.GetPath()}");
        if (jsonResponse != null)
        {
            new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(jsonResponse);
            using (new Div(writer).WithClass("text-center").End())
            {
                TextSaveButton.Write(writer, FileName, jsonResponse);
            }
        }
        else
        {
            writer.Write("JSON data could not be loaded.");
        }
    }
}
