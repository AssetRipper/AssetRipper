using AssetRipper.Assets;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class DataTab : HtmlTab
{
    private readonly HttpClient httpClient;
    private readonly IUnityObjectBase asset;

    public string FileName { get; } = $"{asset.GetBestName()}.data";
    public override string DisplayName => Localization.Data;
    public override string HtmlName => "data";

    public DataTab(IUnityObjectBase asset, HttpClient httpClient)
    {
        this.asset = asset;
        this.httpClient = httpClient;
    }

    public override async Task WriteAsync(TextWriter writer)
    {
        var dataResponse = await httpClient.GetFromJsonAsync<byte[]>($"/Assets/Data?assetPath={asset.GetPath()}");
        if (dataResponse != null)
        {
            DataSaveButton.Write(writer, FileName, dataResponse);
        }
        else
        {
            writer.Write("Binary data could not be loaded.");
        }
    }
}
