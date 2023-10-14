using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_1127;
using AssetRipper.SourceGenerated.Classes.ClassID_329;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class VideoClipExportCollection : AssetExportCollection<IVideoClip>
	{
		public VideoClipExportCollection(VideoClipExporter assetExporter, IVideoClip asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return GetExtensionFromPath(Asset.OriginalPath_R);
		}

		private static string GetExtensionFromPath(string path)
		{
			string extension = Path.GetExtension(path);
			return string.IsNullOrEmpty(extension) ? "bytes" : extension.Substring(1);
		}

		protected override IVideoClipImporter CreateImporter(IExportContainer container)
		{
			//There may be more fields to set here. I did not throughly check.

			IVideoClipImporter importer = VideoClipImporter.Create(container.File, container.ExportVersion);
			importer.EndFrame = (int)Asset.FrameCount;
			if (importer.Has_SourceFileSize())
			{
				importer.OriginalHeight = (int)Asset.Height;
				importer.OriginalWidth = (int)Asset.Width;
				importer.SourceFileSize = Asset.ExternalResources.Size;
			}
			importer.FrameRate = Asset.FrameRate;
			importer.FrameCount = importer.EndFrame;
			importer.ImportAudio = true;
			if (importer.Has_AssetBundleName_R() && Asset.AssetBundleName is not null)
			{
				importer.AssetBundleName_R = Asset.AssetBundleName;
			}
			return importer;
		}
	}
}
