using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using System.IO;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class FontAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			if (asset is IFont font)
				return IsValidData(font.RawData);
			else
				return false;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension((IFont)asset));
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			using FileStream stream = File.Create(path);
			using BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(((IFont)asset).RawData);
			return true;
		}

		string GetExportExtension(IFont font)
		{
			byte[] fontData = font.RawData;
			uint type = System.BitConverter.ToUInt32(fontData, 0);
			return type == OttoAsciiFourCC ? "otf" : "ttf";
		}

		/// <summary>
		/// OTTO ascii
		/// </summary>
		private const int OttoAsciiFourCC = 0x4F54544F;
	}
}
