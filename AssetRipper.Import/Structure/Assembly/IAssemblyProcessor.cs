using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Import.Structure.Assembly;

public interface IAssemblyProcessor
{
	void Process(IAssemblyManager manager);
}
