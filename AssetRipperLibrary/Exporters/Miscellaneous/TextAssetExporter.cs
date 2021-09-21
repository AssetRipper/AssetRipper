using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using System.IO;
using System.Text.Json;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		private TextExportMode exportMode;
		public TextAssetExporter(LibraryConfiguration configuration)
		{
			exportMode = configuration.TextExportMode;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension(asset));
		}

		public override bool Export(IExportContainer container, Object asset, string path)
		{
			using (Stream stream = FileUtils.CreateVirtualFile(path))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(((TextAsset)asset).Script);
				}
			}
			return true;
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
