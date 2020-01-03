using System.Collections.Generic;

namespace uTinyRipper.Classes
{
	public interface IDependent
	{
		IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context);
	}
}
