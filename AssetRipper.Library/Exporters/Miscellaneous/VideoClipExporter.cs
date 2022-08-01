using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using IVideoClip327 = AssetRipper.SourceGenerated.Classes.ClassID_327.IVideoClip;
using IVideoClip329 = AssetRipper.SourceGenerated.Classes.ClassID_329.IVideoClip;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class VideoClipExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return IsValidVideoClip329(asset) || IsValidVideoClip327(asset);
		}

		private static bool IsValidVideoClip327(IUnityObjectBase asset)
		{
			return asset is IVideoClip327 clip && clip.ExternalResources_C327.CheckIntegrity(clip.SerializedFile);
		}

		private static bool IsValidVideoClip329(IUnityObjectBase asset)
		{
			return asset is IVideoClip329 clip && clip.ExternalResources_C329.CheckIntegrity(clip.SerializedFile);
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new VideoClipExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			if (TryGetData(asset, out byte[]? data))
			{
				TaskManager.AddTask(File.WriteAllBytesAsync(path, data));
				return true;
			}
			else
			{
				return false;
			}
		}

		private static bool TryGetData(IUnityObjectBase clip, [NotNullWhen(true)] out byte[]? data)
		{
			if (clip is IVideoClip329 videoClip329 && videoClip329.ExternalResources_C329.TryGetContent(videoClip329.SerializedFile, out data))
			{
				return true;
			}
			else if (clip is IVideoClip327 videoClip327 && videoClip327.ExternalResources_C327.TryGetContent(videoClip327.SerializedFile, out data))
			{
				return true;
			}
			else
			{
				data = null;
				return false;
			}
		}
	}
}
