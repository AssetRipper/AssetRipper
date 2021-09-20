using AssetRipper.Core.Classes.Font;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using System.IO;

namespace AssetRipper.Core.Project.Exporters
{
	public sealed class FontAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(Object asset)
		{
			if (asset is Font font)
				return font.IsValidData;
			else
				return false;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension((Font)asset));
		}

		public override bool Export(IExportContainer container, Object asset, string path)
		{
			using (Stream stream = FileUtils.CreateVirtualFile(path))
			{
				if (Font.HasFontData(container.Version))
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						writer.Write(((Font)asset).FontData);
					}
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		string GetExportExtension(Font font)
		{
			byte[] fontData = font.FontData;
			uint type = System.BitConverter.ToUInt32(fontData, 0);
			return type == OttoAsciiFourCC ? "otf" : "ttf";
		}

		/// <summary>
		/// OTTO ascii
		/// </summary>
		private const int OttoAsciiFourCC = 0x4F54544F;
	}
}
