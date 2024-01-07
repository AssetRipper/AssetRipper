using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_329;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class VideoClipExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IVideoClip clip && clip.ExternalResources.CheckIntegrity(clip.Collection))
			{
				exportCollection = new VideoClipExportCollection(this, clip);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
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
			if (clip is IVideoClip videoClip329 && videoClip329.ExternalResources.TryGetContent(videoClip329.Collection, out data))
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
