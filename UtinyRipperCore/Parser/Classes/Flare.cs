using System.Collections.Generic;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
