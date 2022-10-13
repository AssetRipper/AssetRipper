using AssetRipper.SourceGenerated;

namespace AssetRipper.Library.Processors.AnimationClips
{
	public sealed partial class AnimationClipConverter
	{
		//default vector3 is 1/3, 1/3, 1/3
		//default quaternion is 1/3, 1/3, 1/3, 1/3

		private readonly struct CurveData
		{
			public readonly string path;
			public readonly string attribute;
			public readonly ClassIDType classId;
			public readonly int fileId;
			public readonly long pathId;

			public CurveData(string path, string attribute, ClassIDType classId) : this()
			{
				this.path = path;
				this.attribute = attribute;
				this.classId = classId;
			}

			public CurveData(string path, string attribute, ClassIDType classId, int fileId, long pathId)
			{
				this.path = path;
				this.attribute = attribute;
				this.classId = classId;
				this.fileId = fileId;
				this.pathId = pathId;
			}
		}
	}
}
