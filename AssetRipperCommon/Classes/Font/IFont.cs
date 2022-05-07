using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Font
{
	public interface IFont : IUnityObjectBase, IHasNameString
	{
		byte[] FontData { get; set; }
	}
}
