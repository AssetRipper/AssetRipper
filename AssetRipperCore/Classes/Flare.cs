using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using System.Collections.Generic;

namespace AssetRipper.Classes
{
	public sealed class Flare : NamedObject
	{
		public Flare(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			throw new System.NotImplementedException();
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
