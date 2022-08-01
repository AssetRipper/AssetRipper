using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Subclasses.StreamedResource;

namespace AssetRipper.Library.Exporters.Audio
{
	public sealed class YamlAudioExportCollection : AssetExportCollection
	{
		public YamlAudioExportCollection(IAssetExporter assetExporter, IAudioClip asset) : base(assetExporter, asset)
		{
		}

		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			IAudioClip asset = (IAudioClip)Asset;
			IStreamedResource? resource = asset.Resource_C83;
			if (resource is not null)
			{
				byte[] originalSource = resource.Source.Data;
				ulong originalOffset = resource.Offset;
				ulong originalSize = resource.Size;
				if (resource.TryGetContent(asset.SerializedFile, out byte[]? data))
				{
					string resPath = filePath + ".resS";
					System.IO.File.WriteAllBytes(resPath, data);
					resource.Source.String = System.IO.Path.GetRelativePath(dirPath, resPath);
				}
				else
				{
					resource.Source.Data = Array.Empty<byte>();
					resource.Offset = 0;
					resource.Size = 0;
				}
				bool result = base.ExportInner(container, filePath, dirPath);
				resource.Source.Data = originalSource;
				resource.Offset = originalOffset;
				resource.Size = originalSize;
				return result;
			}
			else
			{
				return base.ExportInner(container, filePath, dirPath);
			}
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "audioclip";
	}
}
