using AssetRipper.Core;
using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using System.IO;

namespace AssetRipper.Library.Exporters.Audio
{
	public class NativeAudioExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is SourceGenerated.Classes.ClassID_83.IAudioClip;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, GetExportExtension((SourceGenerated.Classes.ClassID_83.IAudioClip)asset));
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			SourceGenerated.Classes.ClassID_83.IAudioClip audioClip = (SourceGenerated.Classes.ClassID_83.IAudioClip)asset;

			byte[] data = audioClip.GetAudioData();
			if (data.IsNullOrEmpty())
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

		private static string GetExportExtension(SourceGenerated.Classes.ClassID_83.IAudioClip audioClip)
		{
			if (audioClip.Has_Type_C83())
			{
				return ((FMODSoundType)audioClip.Type_C83).ToRawExtension();
			}
			else
			{
				return ((AudioCompressionFormat)audioClip.CompressionFormat_C83).ToRawExtension();
			}
		}
	}
}
