using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public sealed class UnknownObject : RawDataObject, IHasNameString
	{
		public string NameString
		{
			get => $"Unknown{AssetClassName}_{RawDataHash:X}";
			set { }
		}

		[NotNull]
		public override string? OriginalAssetPath
		{
			get => Path.Combine("AssetRipper", "UnknownAssets", AssetClassName, NameString);
			set { }
		}

		public UnknownObject(AssetInfo assetInfo) : base(assetInfo) { }
	}
}
