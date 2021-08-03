using AssetRipper.Core.Classes.Misc;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Utils
{
	public static class GuidUtils
	{
		public static void RandomizeAssetGuid(IEnumerable<Object> assets)
		{
			foreach (var asset in assets)
			{
				asset.AssetInfo.GUID = new UnityGUID(Guid.NewGuid());
			}
		}

		public static void SetGUID(Object asset, byte[] guid)
		{
			var swapped = new byte[guid.Length];
			for (int i = 0; i < guid.Length; i++)
			{
				var x = guid[i];
				swapped[i] = (byte)((x & 0x0F) << 4 | (x & 0xF0) >> 4);
			}
			asset.AssetInfo.GUID = new UnityGUID(swapped);
		}
	}
}
