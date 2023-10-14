using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Import.AssetCreation
{
	public sealed class UnreadableObject : RawDataObject, INamed
	{
		private Utf8String? name;

		[AllowNull]
		public Utf8String Name
		{
			get
			{
				return Utf8String.IsNullOrEmpty(name)
					? $"Unreadable{ClassName}_{RawDataHash:X}"
					: name;
			}

			set => name = value;
		}

		public UnreadableObject(AssetInfo assetInfo, byte[] data) : base(assetInfo, data) { }
	}
}
