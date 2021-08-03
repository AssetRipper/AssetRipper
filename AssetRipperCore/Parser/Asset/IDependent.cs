using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Asset
{
	public interface IDependent
	{
		IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context);
	}
}
