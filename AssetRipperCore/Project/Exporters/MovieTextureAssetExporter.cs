using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using System.IO;

namespace AssetRipper.Core.Project.Exporters
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(Object asset)
		{
			return HasData(asset.BundleUnityVersion);
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset, "ogv");
		}

		public override bool Export(IExportContainer container, Object asset, string path)
		{
			using (Stream stream = FileUtils.CreateVirtualFile(path))
			{
				if (HasData(container.Version))
				{
					MovieTexture movieTexture = (MovieTexture)asset;
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						writer.Write(movieTexture.MovieData, 0, movieTexture.MovieData.Length);
					}
					return true;
				}
				else
				{
					Logger.Log(LogType.Warning, LogCategory.Export, "Movie texture doesn't have any data");
					return false;
				}
			}
		}

		private static bool HasData(UnityVersion version) => MovieTexture.HasData(version) || MovieTexture.IsInherited(version);
	}
}
