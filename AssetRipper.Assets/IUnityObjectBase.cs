using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets
{
	public interface IUnityObjectBase
	{
		AssetInfo AssetInfo { get; }
		int ClassID { get; }
		string ClassName { get; }
		AssetCollection Collection { get; }
		long PathID { get; }
	}
}