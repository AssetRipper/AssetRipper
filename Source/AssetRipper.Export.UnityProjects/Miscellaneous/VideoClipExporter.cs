using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Extensions;
using IVideoClip327 = AssetRipper.SourceGenerated.Classes.ClassID_327.IVideoClip;
using IVideoClip329 = AssetRipper.SourceGenerated.Classes.ClassID_329.IVideoClip;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class VideoClipExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (IsValidVideoClip329(asset) || IsValidVideoClip327(asset))
			{
				exportCollection = new VideoClipExportCollection(this, asset);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}

		private static bool IsValidVideoClip327(IUnityObjectBase asset)
		{
			return asset is IVideoClip327 clip && clip.ExternalResources_C327.CheckIntegrity(clip.Collection);
		}

		private static bool IsValidVideoClip329(IUnityObjectBase asset)
		{
			return asset is IVideoClip329 clip && clip.ExternalResources_C329.CheckIntegrity(clip.Collection);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			if (TryGetData(asset, out byte[]? data))
			{
				File.WriteAllBytes(path, data);
				return true;
			}
			else
			{
				return false;
			}
		}

		private static bool TryGetData(IUnityObjectBase clip, [NotNullWhen(true)] out byte[]? data)
		{
			if (clip is IVideoClip329 videoClip329 && videoClip329.ExternalResources_C329.TryGetContent(videoClip329.Collection, out data))
			{
				return true;
			}
			else if (clip is IVideoClip327 videoClip327 && videoClip327.ExternalResources_C327.TryGetContent(videoClip327.Collection, out data))
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
