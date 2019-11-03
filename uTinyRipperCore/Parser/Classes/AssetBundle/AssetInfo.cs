using System.Collections.Generic;

namespace uTinyRipper.Classes.AssetBundles
{
	public struct AssetInfo : IAssetReadable, IDependent
	{
		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool HasPreload(Version version) => version.IsGreaterEqual(2, 5);

		public void Read(AssetReader reader)
		{
			if (HasPreload(reader.Version))
			{	
				PreloadIndex = reader.ReadInt32();
				PreloadSize = reader.ReadInt32();
			}
			Asset.Read(reader);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Asset, "asset");
		}

		public int PreloadIndex { get; set; }
		public int PreloadSize { get; set; }

		public PPtr<Object> Asset;
	}
}
