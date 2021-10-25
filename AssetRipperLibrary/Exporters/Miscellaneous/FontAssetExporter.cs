using AssetRipper.Core;
using AssetRipper.Core.Classes.Object;
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
		public override bool IsHandle(UnityObjectBase asset)
		{
			if (asset is IFontAsset font)
				return IsValidData(font.RawData);
			else
				return false;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension((IFontAsset)asset));
		}

		public override bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			using (Stream stream = FileUtils.CreateVirtualFile(path))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(((IFontAsset)asset).RawData);
				}
				return true;
			}
		}

		string GetExportExtension(IFontAsset font)
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
