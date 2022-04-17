using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Font
{
	public interface IFont : IUnityObjectBase, IHasName
	{
		byte[] FontData { get; set; }
	}
}
