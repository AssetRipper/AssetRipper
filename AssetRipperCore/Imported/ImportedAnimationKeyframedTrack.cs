using AssetRipper.Core.Math;
using System.Collections.Generic;

namespace AssetRipper.Core.Imported
{
	public class ImportedAnimationKeyframedTrack
	{
		public string Path { get; set; }
		public List<ImportedKeyframe<Vector3f>> Scalings = new List<ImportedKeyframe<Vector3f>>();
		public List<ImportedKeyframe<Vector3f>> Rotations = new List<ImportedKeyframe<Vector3f>>();
		public List<ImportedKeyframe<Vector3f>> Translations = new List<ImportedKeyframe<Vector3f>>();
		public ImportedBlendShape BlendShape;
	}
}
