using System.Collections.Generic;

namespace uTinyRipper.Classes
{
	public sealed class Flare : NamedObject
	{
		public Flare(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			throw new System.NotImplementedException();
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
