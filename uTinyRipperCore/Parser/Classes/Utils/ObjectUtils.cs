using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace uTinyRipper.Classes
{
	public static class ObjectUtils
	{
		public static List<Object> CollectDependencies(Object asset, bool isLog = false)
		{
			HashSet<Object> hdeps = new HashSet<Object>();
			List<Object> deps = new List<Object>();
			hdeps.Add(asset);
			deps.Add(asset);

			for (int i = 0; i < deps.Count; i++)
			{
				Object dep = deps[i];
				foreach (Object newDep in dep.FetchDependencies(dep.File, isLog))
				{
					if (newDep == null)
					{
						continue;
					}

					if (hdeps.Add(newDep))
					{
						deps.Add(newDep);
					}
				}
			}
			
			return deps;
		}

		public static long GenerateExportID(Object asset, IEnumerable<long> exportIDs)
		{
			return GenerateExportID(asset, (id) => exportIDs.Any(t => t == id));
		}

		public static long GenerateExportID(Object asset, Func<long, bool> uniqueChecker)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

			ThreadSafeRandom random = new ThreadSafeRandom();
			uint classID = (uint)asset.ClassID;
#if DEBUG
			int length = BitConverterExtensions.GetDigitsCount(classID);
			if (length > 4)
			{
				throw new NotSupportedException($"Class ID {classID} with more that 4 digits isn't supported");
			}
#endif
			long prefix = classID * 1000000000000000L;
			ulong persistentValue = 0;
#warning TODO: depending on the export version exportID should has random or ordered value
			long exportID = 0;
			do
			{
				ulong value = 0;
				value += unchecked((uint)random.Next(0, 100000)) * 10000000000UL;
				value += unchecked((uint)random.Next(0, 100000)) * 100000UL;
				value += unchecked((uint)random.Next(0, 100000)) * 1UL;
				persistentValue = unchecked(persistentValue + value);
				exportID = prefix + (long)(persistentValue % 1000000000000000L);

			}
			while (uniqueChecker(exportID));
			return exportID;
		}

		public static EngineGUID CalculateAssetsGUID(IEnumerable<Object> assets)
		{
			List<uint> hashList = new List<uint>();
			foreach (Object asset in assets)
			{
				hashList.Add(asset.GUID.Data0);
				hashList.Add(asset.GUID.Data1);
				hashList.Add(asset.GUID.Data2);
				hashList.Add(asset.GUID.Data3);
			}

			return CalculateGUID(hashList);
		}

		public static EngineGUID CalculateGUID(List<uint> hashList)
		{
			uint[] hashArray = hashList.ToArray();
			byte[] buffer = new byte[hashArray.Length * sizeof(uint)];
			Buffer.BlockCopy(hashArray, 0, buffer, 0, buffer.Length);
			using (MD5 md5 = MD5.Create())
			{
				byte[] hash = md5.ComputeHash(buffer);
				return new EngineGUID(hash);
			}
		}
	}
}
