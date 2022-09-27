using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.Linq;
using System.Text.Json;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class TextAssetExportCollection : AssetExportCollection
	{
		private const string JsonExtension = "json";
		private const string TxtExtension = "txt";
		private const string BytesExtension = "bytes";

		public TextAssetExportCollection(TextAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			if (!string.IsNullOrEmpty(asset.OriginalExtension))
			{
				return asset.OriginalExtension;
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
	}
}
