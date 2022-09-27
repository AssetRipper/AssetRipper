using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes
{
	public sealed class UnknownObject : RawDataObject, IHasNameString
	{
		public string NameString
		{
			get => $"Unknown{AssetClassName}_{RawDataHash:X}";
			set { }
		}

		public UnknownObject(AssetInfo assetInfo) : base(assetInfo) { }
	}
}
