using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace UtinyRipper.Classes
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

		public static ulong GenerateExportID(Object asset, IEnumerable<ulong> exportIDs)
		{
			return GenerateExportID(asset, (id) => exportIDs.Any(t => t == id));
		}

		public static ulong GenerateExportID(Object asset, Func<ulong, bool> uniqueChecker)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

#warning TODO: values acording to read version (current 2017.3.0f3)
			ulong exportID;
			do
			{
				uint classID = (uint)asset.ClassID;
#if DEBUG
				int length = BitConverterExtensions.GetDigitsCount(classID);
				if (length > 4)
				{
					throw new NotSupportedException($"Class ID {classID} with more that 4 digits isn't supported");
				}
#endif
				exportID = classID;
				exportID *= 1000000000000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 100000000000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 10000000000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 1000000000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 100000000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 10000000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 1000000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 100000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 10000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 1000000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 100000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 10000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 1000UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 100UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 10UL;
				exportID |= unchecked((ulong)RandomUtils.Next(0, 2)) * 1UL;
			}
			while (uniqueChecker(exportID));
			return exportID;
		}

		public static EngineGUID CalculateObjectsGUID(IEnumerable<Object> asset)
		{
			List<uint> hashList = new List<uint>();
			foreach (Object @object in asset)
			{
				hashList.Add(@object.GUID.Data0);
				hashList.Add(@object.GUID.Data1);
				hashList.Add(@object.GUID.Data2);
				hashList.Add(@object.GUID.Data3);
			}

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
