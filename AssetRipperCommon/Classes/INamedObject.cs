using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface INamedObject : IHasName
	{
		string ValidName { get; }
	}
}