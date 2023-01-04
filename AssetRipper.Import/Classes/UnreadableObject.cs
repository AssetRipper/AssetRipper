using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Import.Classes
{
	public sealed class UnreadableObject : RawDataObject, IHasNameString
	{
		private string nameString = "";

		public string NameString
		{
			get
			{
				return string.IsNullOrWhiteSpace(nameString)
					? $"Unreadable{ClassName}_{RawDataHash:X}"
					: nameString;
			}

			set => nameString = value;
		}

		public UnreadableObject(AssetInfo assetInfo) : base(assetInfo) { }
	}
}
