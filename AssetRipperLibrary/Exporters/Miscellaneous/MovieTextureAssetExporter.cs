using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using System.IO;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(Object asset)
		{
			if (asset is IMovieTexture texture)
				return IsValidData(texture.RawData);
			else
				return false;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset, "ogv");
		}

		public override bool Export(IExportContainer container, Object asset, string path)
		{
			using (Stream stream = FileUtils.CreateVirtualFile(path))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(((IMovieTexture)asset).RawData);
				}
				return true;
			}
		}

		static bool IsValidData(byte[] data) => data != null && data.Length > 0;
	}
}
