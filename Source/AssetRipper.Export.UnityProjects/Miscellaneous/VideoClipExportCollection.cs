using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
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
			return GetExtensionFromPath(Asset.OriginalPath_C329.String);
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
			importer.EndFrame_C1127 = (int)Asset.FrameCount_C329;
			if (importer.Has_SourceFileSize_C1127())
			{
				importer.OriginalHeight_C1127 = (int)Asset.Height_C329;
				importer.OriginalWidth_C1127 = (int)Asset.Width_C329;
				importer.SourceFileSize_C1127 = Asset.ExternalResources_C329.Size;
			}
			importer.FrameRate_C1127 = Asset.FrameRate_C329;
			importer.FrameCount_C1127 = importer.EndFrame_C1127;
			importer.ImportAudio_C1127 = true;
			if (importer.Has_AssetBundleName_C1127() && Asset.AssetBundleName is not null)
			{
				importer.AssetBundleName_C1127 = Asset.AssetBundleName;
			}
			return importer;
		}
	}
}
