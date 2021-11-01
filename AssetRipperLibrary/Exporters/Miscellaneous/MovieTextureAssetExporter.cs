using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using System.IO;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			if (asset is IMovieTexture texture)
				return IsValidData(texture.RawData);
			else
				return false;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, "ogv");
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
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
	}
}
