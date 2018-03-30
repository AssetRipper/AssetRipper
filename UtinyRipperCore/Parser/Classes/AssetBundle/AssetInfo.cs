using System.Collections.Generic;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AssetBundles
{
	public struct AssetInfo : IAssetReadable
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
			Object @object = Asset.FindObject(file);
			if(@object == null)
			{
				if(isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"AssetInfo's asset {Asset.ToLogString(file)} wasn't found ");
				}
			}
			else
			{
				yield return @object;
			}
		}

		public int PreloadIndex { get; private set; }
		public int PreloadSize { get; private set; }

		public PPtr<Object> Asset;
	}
}
