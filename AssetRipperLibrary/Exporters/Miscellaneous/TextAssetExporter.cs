using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
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

		public override bool IsHandle(UnityObjectBase asset)
		{
			if (asset is ITextAsset textAsset)
				return IsValidData(textAsset.RawData);
			else
				return false;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension(asset));
		}

		public override bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			using (Stream stream = FileUtils.CreateVirtualFile(path))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(((ITextAsset)asset).RawData);
				}
			}
			return true;
		}

		private string GetExportExtension(UnityObjectBase asset)
		{
			switch (exportMode)
			{
				case TextExportMode.Txt:
					return "txt";
				case TextExportMode.Parse:
					return GetExtension((ITextAsset)asset);
				case TextExportMode.Bytes:
				default:
					return "bytes";
			}
		}

		private static string GetExtension(ITextAsset asset)
		{
			if (IsValidJson(asset.Text))
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
