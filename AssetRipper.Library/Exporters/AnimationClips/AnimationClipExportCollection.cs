using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.AnimationClips
{
	public sealed class AnimationClipExportCollection : AssetExportCollection
	{
		private static HashSet<IAnimationClip> convertedClips = new();
		public AnimationClipExportCollection(IAssetExporter assetExporter, IAnimationClip asset) : base(assetExporter, asset)
		{
		}

		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			IAnimationClip animationClip = (IAnimationClip)Asset;
			if (!convertedClips.Contains(animationClip))
			{
				//this permanently alters the asset and should only be done once
				AnimationClipConverter.Process(animationClip);
				convertedClips.Add(animationClip);
			}
			return base.ExportInner(container, filePath, dirPath);
		}
	}
}
