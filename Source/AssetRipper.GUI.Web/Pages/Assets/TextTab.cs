using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Decompilation.CSharp;
using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class TextTab : HtmlTab
{
    private readonly HttpClient httpClient;
    private readonly IUnityObjectBase asset;

    public string? FileName { get; }

    public override string DisplayName => Localization.AssetTabText;

    public override string HtmlName => "text";

    public TextTab(IUnityObjectBase asset, HttpClient httpClient)
    {
        this.asset = asset;
        this.httpClient = httpClient;
        FileName = GetFileName(asset);
    }

    public override async Task WriteAsync(TextWriter writer)
    {
        var textResponse = await httpClient.GetFromJsonAsync<string>($"/Assets/Text?assetPath={asset.GetPath()}");
        if (textResponse != null)
        {
            new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(textResponse);
            using (new Div(writer).WithClass("text-center").End())
            {
                TextSaveButton.Write(writer, FileName, textResponse);
            }
        }
        else
        {
            writer.Write("Text data could not be loaded.");
        }
    }

    public static string GetFileName(IUnityObjectBase asset)
    {
        return asset switch
        {
            IShader => $"{asset.GetBestName()}.shader",
            IMonoScript monoScript => $"{monoScript.ClassName_R}.cs",
            ITextAsset textAsset => $"{asset.GetBestName()}.{GetTextAssetExtension(textAsset)}",
            _ => $"{asset.GetBestName()}.txt",
        };

        static string GetTextAssetExtension(ITextAsset textAsset)
        {
            return string.IsNullOrEmpty(textAsset.OriginalExtension) ? "txt" : textAsset.OriginalExtension;
        }
    }
}
