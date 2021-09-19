using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using System.Text.Json;

namespace AssetRipper.Core.Project.Exporters
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		private TextExportMode exportMode;
		public TextAssetExporter(CoreConfiguration configuration)
		{
			exportMode = configuration.TextExportMode;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension(asset));
		}

		private string GetExportExtension(Object asset)
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
				using (var parsed = JsonDocument.Parse(text))
				{
					return parsed != null;
				}
			}
			catch { }
			return false;
		}
	}
}
