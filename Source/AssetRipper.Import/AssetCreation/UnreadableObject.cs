using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Import.AssetCreation
{
	public sealed class UnreadableObject : RawDataObject, INamed
	{
		[AllowNull]
		[field: MaybeNull]
		public Utf8String Name
		{
			get
			{
				return Utf8String.IsNullOrEmpty(field)
					? $"Unreadable{ClassName}_{RawDataHash:X}"
					: field;
			}

			set;
		}

		public UnreadableObject(AssetInfo assetInfo, byte[] data) : base(assetInfo, data) { }
	}
}
