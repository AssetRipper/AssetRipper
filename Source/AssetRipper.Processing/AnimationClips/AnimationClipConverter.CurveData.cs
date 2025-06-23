using AssetRipper.Assets;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Processing.AnimationClips;

public partial struct AnimationClipConverter
{
	private readonly record struct CurveData(string Path, string Attribute, ClassIDType ClassID, IUnityObjectBase? Script = null)
	{
	}
}
