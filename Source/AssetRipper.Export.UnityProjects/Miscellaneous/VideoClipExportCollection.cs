using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_329;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class VideoClipExportCollection : AssetExportCollection<IUnityObjectBase>
	{
		public VideoClipExportCollection(VideoClipExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			if (asset is IVideoClip videoClip)
			{
				return GetExtensionFromPath(videoClip.OriginalPath_C329.String);
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
