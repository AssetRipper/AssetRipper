using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.Classes.Object;
using System.Collections.Generic;

namespace AssetRipper.Parser.Asset
{
	public interface IDependent
	{
		IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context);
	}
}
