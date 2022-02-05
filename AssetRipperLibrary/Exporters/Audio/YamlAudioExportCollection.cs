using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;

namespace AssetRipper.Library.Exporters.Audio
{
	public sealed class YamlAudioExportCollection : AssetExportCollection
	{
		public YamlAudioExportCollection(IAssetExporter assetExporter, IAudioClip asset) : base(assetExporter, asset)
		{
		}

		protected override bool ExportInner(ProjectAssetContainer container, string filePath, string dirPath)
		{
			IAudioClip asset = (IAudioClip)Asset;
			IStreamedResource resource = asset.Resource;
			if (resource != null && resource.TryGetContent(asset.SerializedFile, out byte[] data))
			{
				string resPath = filePath + ".resS";
				System.IO.File.WriteAllBytes(resPath, data);
				resource.Source = System.IO.Path.GetRelativePath(dirPath, resPath);
			}

			return base.ExportInner(container, filePath, dirPath);
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return "audioclip";
		}
	}
}
