using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project.Collections;
using IVideoClip327 = AssetRipper.SourceGenerated.Classes.ClassID_327.IVideoClip;
using IVideoClip329 = AssetRipper.SourceGenerated.Classes.ClassID_329.IVideoClip;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class VideoClipExportCollection : AssetExportCollection<IUnityObjectBase>
	{
		public VideoClipExportCollection(VideoClipExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			if (asset is IVideoClip329 videoClip329)
			{
				return GetExtensionFromPath(videoClip329.OriginalPath_C329.String);
			}
			else if (asset is IVideoClip327 videoClip327)
			{
				return GetExtensionFromPath(videoClip327.OriginalPath_C327.String);
			}
			else
			{
				return base.GetExportExtension(asset);
			}
		}

		private static string GetExtensionFromPath(string path)
		{
			string extension = Path.GetExtension(path);
			return string.IsNullOrEmpty(extension) ? "bytes" : extension.Substring(1);
		}
	}
}
