using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Core.Project
{
	public static class ExportIdHandler
	{
		public static long GetMainExportID(UnityObjectBase asset)
		{
			return GetMainExportID((uint)asset.ClassID, 0);
		}

		public static long GetMainExportID(uint classID)
		{
			return GetMainExportID(classID, 0);
		}

		public static long GetMainExportID(UnityObjectBase asset, uint value)
		{
			return GetMainExportID((uint)asset.ClassID, value);
		}

		public static long GetMainExportID(uint classID, uint value)
		{
			if (classID > 100100)
			{
				if (value != 0)
				{
					throw new ArgumentException("Unique asset type with non unique modifier", nameof(value));
				}
				return classID;
			}

#if DEBUG
			int digits = BitConverterExtensions.GetDigitsCount(value);
			if (digits > 5)
			{
				throw new ArgumentException($"Value {value} for main export ID must have no more than 5 digits");
			}
#endif
			return classID * 100000 + value;
		}
	}
}
