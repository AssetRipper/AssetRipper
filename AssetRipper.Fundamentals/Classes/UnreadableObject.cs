using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public sealed class UnreadableObject : RawDataObject, IHasNameString
	{
		private string nameString = "";

		public string NameString
		{
			get
			{
				return string.IsNullOrWhiteSpace(nameString)
					? $"Unreadable{AssetClassName}_{RawDataHash:X}"
					: nameString;
			}

			set => nameString = value;
		}

		[NotNull]
		public override string? OriginalAssetPath
		{
			get => Path.Combine("AssetRipper", "UnreadableAssets", AssetClassName, NameString);
			set { }
		}

		public UnreadableObject(AssetInfo assetInfo) : base(assetInfo) { }
	}
}
