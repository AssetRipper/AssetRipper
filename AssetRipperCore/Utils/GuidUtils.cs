using AssetRipper.Core.Classes.Misc;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Utils
{
	public static class GuidUtils
	{
		public static Guid GetNewGuid() => Guid.NewGuid();
		/// <returns>A new guid in hexadecimal characters without any dashes</returns>
		public static string GetNewGuidString() => Guid.NewGuid().ToString().Replace("-","");
		/// <summary> Get the first characters of a new random guid </summary>
		/// <param name="numCharacters">The number of characters to return up to 32</param>
		/// <returns>A new string of pseudorandom hexadecimal characters</returns>
		public static string GetNewGuidString(int numCharacters)
		{
			string guid = GetNewGuidString();
			if (numCharacters < 1 || numCharacters > 31)
				return guid;
			else
				return guid.Substring(0, numCharacters);
		}
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
