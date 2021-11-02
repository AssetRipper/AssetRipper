using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Asset
{
	public interface IDependent
	{
		IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context);
	}
}
