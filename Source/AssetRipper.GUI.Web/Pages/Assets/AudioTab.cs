using AssetRipper.Assets;
using System.Net.Http;
using System.Net.Http.Json;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class AudioTab : HtmlTab
{
    private readonly IUnityObjectBase asset;
    private readonly HttpClient httpClient;

    public override string DisplayName => Localization.AssetTabAudio;

    public override string HtmlName => "audio";

    public AudioTab(IUnityObjectBase asset, HttpClient httpClient)
    {
        this.asset = asset;
        this.httpClient = httpClient;
    }

    public override async Task WriteAsync(TextWriter writer)
    {
        var audioResponse = await httpClient.GetFromJsonAsync<string>($"/Assets/Audio?assetPath={asset.GetPath()}");
        if (audioResponse != null)
        {
            new Audio(writer).WithControls("").WithClass("mt-4").WithSrc(audioResponse).Close();
        }
        else
        {
            writer.Write("Audio could not be loaded.");
        }
    }
}
