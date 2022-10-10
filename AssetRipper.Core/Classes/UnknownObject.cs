using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Core.Classes
{
	public sealed class UnknownObject : RawDataObject, IHasNameString
	{
		public string NameString
		{
			get => $"Unknown{ClassName}_{RawDataHash:X}";
			set { }
		}

		public UnknownObject(AssetInfo assetInfo) : base(assetInfo) { }
	}
}
