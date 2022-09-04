using AssetRipper.Core;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using System.IO;

namespace AssetRipper.Library.Exporters.Audio
{
	public class NativeAudioExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAudioClip;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
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
