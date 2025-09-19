using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_1031;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.Miscellaneous;

public sealed class TextAssetExportCollection : AssetExportCollection<ITextAsset>
{
	private const string JsonExtension = "json";
	private const string TxtExtension = "txt";
	private const string BytesExtension = "bytes";

	public TextAssetExportCollection(TextAssetExporter assetExporter, ITextAsset asset) : base(assetExporter, asset)
	{
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		string? extension = asset.GetBestExtension();
		if (extension is not null)
		{
			return extension;
		}
		return ((TextAssetExporter)AssetExporter).ExportMode switch
		{
			TextExportMode.Txt => TxtExtension,
			TextExportMode.Parse => GetExtension((ITextAsset)asset),
			_ => BytesExtension,
		};
	}

	private static string GetExtension(ITextAsset asset)
	{
		string text = asset.Script_C49.String;
		if (IsValidJson(text))
		{
			return JsonExtension;
		}
		else if (IsPlainText(text))
		{
			return TxtExtension;
		}
		else
		{
			return BytesExtension;
		}
	}

	private static bool IsValidJson(string text)
	{
		try
		{
			using JsonDocument? parsed = JsonDocument.Parse(text);
			return parsed != null;
		}
		catch
		{
			return false;
		}
	}

	private static bool IsPlainText(string text) => text.All(c => !char.IsControl(c) || char.IsWhiteSpace(c));

	protected override ITextScriptImporter CreateImporter(IExportContainer container)
	{
		ITextScriptImporter importer = TextScriptImporter.Create(container.File, container.ExportVersion);
		if (importer.Has_AssetBundleName_R() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_R = Asset.AssetBundleName;
		}
		return importer;
	}
}
