using AssetRipper.Classes.Misc;
using AssetRipper.Classes.Object;
using System.Collections.Generic;

namespace AssetRipper.Parser.Asset
{
	public interface IDependent
	{
		IEnumerable<PPtr<UnityObject>> FetchDependencies(DependencyContext context);
	}
}
