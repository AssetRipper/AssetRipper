using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface INamedObject : IHasName, IUnityObjectBase
	{
		string ValidName { get; }
	}
}