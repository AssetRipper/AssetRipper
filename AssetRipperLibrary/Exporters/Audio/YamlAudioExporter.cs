using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;

namespace AssetRipper.Library.Exporters.Audio
{
	public sealed class YamlAudioExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAudioClip && asset is not AudioClip;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new YamlAudioExportCollection(this, (IAudioClip)asset);
		}
	}
}
