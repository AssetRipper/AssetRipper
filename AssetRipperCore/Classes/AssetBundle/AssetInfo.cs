using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public struct AssetInfo : IAssetReadable, IDependent
	{
		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool HasPreload(UnityVersion version) => version.IsGreaterEqual(2, 5);

		public void Read(AssetReader reader)
		{
			if (HasPreload(reader.Version))
			{
				PreloadIndex = reader.ReadInt32();
				PreloadSize = reader.ReadInt32();
			}
			Asset.Read(reader);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Asset, "asset");
		}

		public int PreloadIndex { get; set; }
		public int PreloadSize { get; set; }

		public PPtr<Object.Object> Asset;
	}
}
