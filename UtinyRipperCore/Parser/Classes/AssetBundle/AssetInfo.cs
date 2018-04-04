using System.Collections.Generic;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AssetBundles
{
	public struct AssetInfo : IAssetReadable, IDependent
	{
		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool IsReadPreload(Version version)
		{
			return version.IsGreaterEqual(2, 5);
		}

		public void Read(AssetStream stream)
		{
			if (IsReadPreload(stream.Version))
			{	
				PreloadIndex = stream.ReadInt32();
				PreloadSize = stream.ReadInt32();
			}
			Asset.Read(stream);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Asset.FetchDependency(file, isLog, () => nameof(AssetInfo), "asset");
		}

		public int PreloadIndex { get; private set; }
		public int PreloadSize { get; private set; }

		public PPtr<Object> Asset;
	}
}
