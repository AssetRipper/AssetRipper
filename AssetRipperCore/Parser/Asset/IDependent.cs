using System.Collections.Generic;

namespace AssetRipper.Classes
{
	public interface IDependent
	{
		IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context);
	}
}
