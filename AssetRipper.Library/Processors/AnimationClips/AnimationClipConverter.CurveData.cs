using AssetRipper.SourceGenerated;

namespace AssetRipper.Library.Processors.AnimationClips
{
	public sealed partial class AnimationClipConverter
	{
		//default vector3 is 1/3, 1/3, 1/3
		//default quaternion is 1/3, 1/3, 1/3, 1/3

		private sealed record class CurveData(string path, string attribute, ClassIDType classId, int fileId = 0, long pathId = 0);
	}
}
