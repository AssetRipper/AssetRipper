using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;

namespace AssetRipper.Core.Utils
{
	public static class ObjectUtils
	{
		private const long TenToTheFifthteenth = 1_000_000_000_000_000L;
		/// <summary>
		/// 9223
		/// </summary>
		private const uint MaxPrefixedClassId = (uint)(long.MaxValue / TenToTheFifthteenth);

		public static long GenerateExportID(IUnityObjectBase asset, Func<long, bool> duplicateChecker)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

#warning TODO: depending on the export version exportID should has random or ordered value

			long exportID;
			uint classID = (uint)asset.ClassID;
			if (classID > MaxPrefixedClassId)
			{
				do
				{
					//Checked for StreamingController on 2018.2.5f1
					//Small class id's use the below format
					//Whereas this uses random id's
					exportID = GenerateInternalID();
				}
				while (duplicateChecker(exportID));
			}
			else
			{
				long prefix = classID * TenToTheFifthteenth;
				ulong persistentValue = 0;
				do
				{
					ulong value = unchecked((ulong)GenerateInternalID());
					persistentValue = unchecked(persistentValue + value);
					exportID = prefix + (long)(persistentValue % TenToTheFifthteenth);
				}
				while (duplicateChecker(exportID));
			}
			
			return exportID;
		}

		public static long GenerateInternalID()
		{
			s_random.NextBytes(s_idBuffer.Value);
			return BitConverter.ToInt64(s_idBuffer.Value, 0);
		}

		public static UnityGUID CalculateAssetsGUID(IEnumerable<IUnityObjectBase> assets)
		{
			List<uint> hashList = new List<uint>();
			foreach (IUnityObjectBase asset in assets)
			{
				hashList.Add(asset.GUID.Data0);
				hashList.Add(asset.GUID.Data1);
				hashList.Add(asset.GUID.Data2);
				hashList.Add(asset.GUID.Data3);
			}

			return CalculateGUID(hashList);
		}

		public static UnityGUID CalculateGUID(List<uint> hashList)
		{
			uint[] hashArray = hashList.ToArray();
			byte[] buffer = new byte[hashArray.Length * sizeof(uint)];
			Buffer.BlockCopy(hashArray, 0, buffer, 0, buffer.Length);
			using MD5 md5 = MD5.Create();
			byte[] hash = md5.ComputeHash(buffer);
			return new UnityGUID(hash);
		}

		public const char DirectorySeparatorChar = '/';
		public const string DirectorySeparator = "/";

		private static readonly ThreadSafeRandom s_random = new ThreadSafeRandom();
		private static readonly ThreadLocal<byte[]> s_idBuffer = new ThreadLocal<byte[]>(() => new byte[8]);
	}
}
