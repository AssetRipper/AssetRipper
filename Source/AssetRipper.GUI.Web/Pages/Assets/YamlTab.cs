using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class YamlTab(IUnityObjectBase asset, HttpClient httpClient) : HtmlTab
{
    private readonly HttpClient httpClient;
    private readonly IUnityObjectBase asset;

    public string FileName { get; } = $"{asset.GetBestName()}.asset";
    public override string DisplayName => Localization.Yaml;
    public override string HtmlName => "yaml";

    public YamlTab(IUnityObjectBase asset, HttpClient httpClient)
    {
        this.asset = asset;
        this.httpClient = httpClient;
    }

    public override async Task WriteAsync(TextWriter writer)
    {
        var yamlResponse = await httpClient.GetFromJsonAsync<string>($"/Assets/Yaml?assetPath={asset.GetPath()}");
        if (yamlResponse != null)
        {
            new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(yamlResponse);
            using (new Div(writer).WithClass("text-center").End())
            {
                TextSaveButton.Write(writer, FileName, yamlResponse);
            }
        }
        else
        {
            writer.Write("YAML data could not be loaded.");
        }
    }
}
