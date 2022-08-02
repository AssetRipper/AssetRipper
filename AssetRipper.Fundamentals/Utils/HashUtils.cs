using System.Security.Cryptography;
using System.Text;

namespace AssetRipper.Core.Utils
{
	public static class HashUtils
	{
		public static string HashBytes(byte[] inputBytes)
		{
			using MD5 md5 = MD5.Create();
			byte[] hashBytes = md5.ComputeHash(inputBytes);
			StringBuilder sb = new();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				sb.Append(hashBytes[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
