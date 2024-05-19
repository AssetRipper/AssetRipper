using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Export.UnityProjects.Utils;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.TextureDecoder.Rgb.Formats;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;
using System.Net.Http;
using System.Net.Http.Json;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class ImageTab : HtmlTab
{
    private readonly IUnityObjectBase asset;
    private readonly HttpClient httpClient;

    public override string DisplayName => Localization.AssetTabImage;

    public override string HtmlName => "image";

    public ImageTab(IUnityObjectBase asset, HttpClient httpClient)
    {
        this.asset = asset;
        this.httpClient = httpClient;
    }

    public override async Task WriteAsync(TextWriter writer)
    {
        var imageResponse = await httpClient.GetFromJsonAsync<DirectBitmap>($"/Assets/Texture?assetPath={asset.GetPath()}");
        if (imageResponse != null)
        {
            MemoryStream stream = new();
            imageResponse.SaveAsPng(stream);
            string sourcePath = $"data:image/png;base64,{stream.ToArray().ToBase64String()}";

            // Click on image to save
            using (new A(writer).WithHref(sourcePath).WithDownload("extracted_image").End())
            {
                new Img(writer).WithSrc(sourcePath).WithStyle("object-fit:contain; width:100%; height:100%").Close();
            }
        }
        else
        {
            writer.Write("Image could not be loaded.");
        }
    }
}
