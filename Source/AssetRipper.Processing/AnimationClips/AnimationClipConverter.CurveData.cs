using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Processing.AnimationClips
{
	public partial struct AnimationClipConverter
	{
		private readonly record struct CurveData(string Path, string Attribute, ClassIDType ClassID, PPtr Script = default)
		{
			public CurveData(string Path, string Attribute, ClassIDType ClassID, IPPtr Script) : this(Path, Attribute, ClassID, Script.ToStruct())
			{
			}
		}
	}
}
