using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		private const string JsonExtension = "json";
		private const string TxtExtension = "txt";
		private const string BytesExtension = "bytes";
		private TextExportMode exportMode;
		public TextAssetExporter(LibraryConfiguration configuration)
		{
			exportMode = configuration.TextExportMode;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			if (asset is ITextAsset textAsset)
				return !textAsset.Script.IsNullOrEmpty();
			else
				return false;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension(asset));
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			TaskManager.AddTask(File.WriteAllBytesAsync(path, ((ITextAsset)asset).Script));
			return true;
		}

		private string GetExportExtension(IUnityObjectBase asset)
		{
			return exportMode switch
			{
				TextExportMode.Txt => TxtExtension,
				TextExportMode.Parse => GetExtension((ITextAsset)asset),
				_ => BytesExtension,
			};
		}

		private static string GetExtension(ITextAsset asset)
		{
			string text = asset.ParseWithUTF8();
			if (IsValidJson(text))
				return JsonExtension;
			else if (IsPlainText(text))
				return TxtExtension;
			else
				return BytesExtension;
		}

		private static bool IsValidJson(string text)
		{
			try
			{
				using var parsed = JsonDocument.Parse(text);
				return parsed != null;
			}
			catch { }
			return false;
		}

		private static bool IsPlainText(string text) => text.All(c => !char.IsControl(c) || char.IsWhiteSpace(c));
	}
}
