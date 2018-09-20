using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

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

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			throw new System.NotImplementedException();
		}
	}
}
