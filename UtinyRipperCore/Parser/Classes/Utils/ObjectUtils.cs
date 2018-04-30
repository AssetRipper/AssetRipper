using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UtinyRipper.Classes
{
	public static class ObjectUtils
	{
		public static List<Object> CollectDependencies(Object @object, bool isLog = false)
		{
			List<Object> deps = new List<Object>();
			deps.Add(@object);

			for (int i = 0; i < deps.Count; i++)
			{
				Object dep = deps[i];
				foreach (Object newDep in dep.FetchDependencies(dep.File, isLog))
				{
					if (newDep == null)
					{
						continue;
					}

					if (!deps.Contains(newDep))
					{
						deps.Add(newDep);
					}
				}
			}
			
			return deps;
		}

		public static string GenerateExportID(Object @object, IEnumerable<string> exportIDs)
		{
			return GenerateExportID(@object, (id) => exportIDs.Any(t => t == id));
		}

		public static string GenerateExportID(Object @object, Func<string, bool> uniqueChecker)
		{
			if (@object == null)
			{
				throw new ArgumentNullException(nameof(@object));
			}

#warning TODO: values acording to read version (current 2017.3.0f3)
			string exportID;
			do
			{
				s_builder.Append((int)@object.ClassID);
				for (int i = 0; i < 15; i++)
				{
					int number = RandomUtils.Next(0, 10);
					s_builder.Append(number);
				}
				exportID = s_builder.ToString();
				s_builder.Length = 0;
			}
			while (uniqueChecker(exportID));
			return exportID;
		}

		public static UtinyGUID CalculateObjectsGUID(IEnumerable<Object> objects)
		{
			List<uint> hashList = new List<uint>();
			foreach (Object @object in objects)
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
				return new UtinyGUID(hash);
			}
		}

		private static readonly StringBuilder s_builder = new StringBuilder();
	}
}
