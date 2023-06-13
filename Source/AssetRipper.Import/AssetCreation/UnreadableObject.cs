using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Import.AssetCreation
{
	public sealed class UnreadableObject : RawDataObject, IHasNameString
	{
		private string nameString = "";

		public string NameString
		{
			get
			{
				return string.IsNullOrEmpty(nameString)
					? $"Unreadable{ClassName}_{RawDataHash:X}"
					: nameString;
			}

			set => nameString = value;
		}

		public UnreadableObject(AssetInfo assetInfo, byte[] data) : base(assetInfo, data) { }
	}
}
