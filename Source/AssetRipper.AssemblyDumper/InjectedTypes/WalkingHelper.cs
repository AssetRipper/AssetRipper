using AssetRipper.Assets;
using AssetRipper.Assets.Traversal;

#nullable disable

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal static class WalkingHelper
{
	public static void WalkComponentPairRelease<T>(int classID, T component, AssetWalker walker) where T : IUnityAssetBase
	{
		KeyValuePair<int, T> pair = new KeyValuePair<int, T>(classID, component);
		if (walker.EnterPair(pair))
		{
			walker.VisitPrimitive(pair.Key);
			walker.DividePair(pair);
			component.WalkRelease(walker);
			walker.ExitPair(pair);
		}
	}

	public static void WalkComponentPairEditor<T>(int classID, T component, AssetWalker walker) where T : IUnityAssetBase
	{
		KeyValuePair<int, T> pair = new KeyValuePair<int, T>(classID, component);
		if (walker.EnterPair(pair))
		{
			walker.VisitPrimitive(pair.Key);
			walker.DividePair(pair);
			component.WalkRelease(walker);
			walker.ExitPair(pair);
		}
	}

	public static void WalkComponentPairStandard<T>(int classID, T component, AssetWalker walker) where T : IUnityAssetBase
	{
		KeyValuePair<int, T> pair = new KeyValuePair<int, T>(classID, component);
		if (walker.EnterPair(pair))
		{
			walker.VisitPrimitive(pair.Key);
			walker.DividePair(pair);
			component.WalkRelease(walker);
			walker.ExitPair(pair);
		}
	}
}
