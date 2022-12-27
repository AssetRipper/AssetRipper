using AssetRipper.Core.Structure.Assembly.Managers;

namespace AssetRipper.Core.Structure.Assembly;

public interface IAssemblyProcessor
{
	void Process(IAssemblyManager manager);
}
