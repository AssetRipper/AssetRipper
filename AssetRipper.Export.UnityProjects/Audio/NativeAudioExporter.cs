using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Import;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Audio
{
	public class NativeAudioExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAudioClip;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new NativeAudioExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IAudioClip audioClip = (IAudioClip)asset;

			byte[] data = audioClip.GetAudioData();
			if (data.Length == 0)
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{audioClip.GetNameNotEmpty()}' because no valid data was found");
				return false;
			}
			else
			{
				TaskManager.AddTask(File.WriteAllBytesAsync(path, data));
				return true;
			}
		}
	}
}
