using System.Collections.Generic;

namespace UtinyRipper
{
	public interface IGameStructure
	{
		IEnumerable<string> FetchFiles();
		IEnumerable<string> FetchAssemblies();

		bool RequestDependency(string dependency);
		bool RequestAssembly(string assembly);

		string Name { get; }
	}
}
