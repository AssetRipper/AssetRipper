using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Project.Exporters;
using System.Text.Json;

namespace AssetRipper.Core.Structure.Collections
{
	public sealed class TextAssetExportCollection : AssetExportCollection
	{
		private TextExportMode exportMode;
		public TextAssetExportCollection(IAssetExporter assetExporter, TextAsset asset, TextExportMode exportMode) : base(assetExporter, asset)
		{
			this.exportMode = exportMode;
		}

		protected override string GetExportExtension(Object asset)
		{
			switch (exportMode)
			{
				case TextExportMode.Txt:
					return "txt";
				case TextExportMode.Parse:
					return GetExtension((TextAsset)asset);
				case TextExportMode.Bytes:
				default:
					return "bytes";
			}
		}

		private static string GetExtension(TextAsset asset)
		{
			if (IsValidJson(asset.TextScript))
				return "json";
			else
				return "txt";
		}

		private static bool IsValidJson(string text)
		{
			try
			{
				using(var parsed = JsonDocument.Parse(text))
				{
					return parsed != null;
				}
			}
			catch { }
			return false;
		}
	}
}
