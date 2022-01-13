using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.VersionHandling
{
	public abstract class UnityHandlerBase
	{
		public UnityVersion UnityVersion { get; protected set; }
		protected Type ClassIDTypeEnum { get; set; }
		public AssetFactoryBase AssetFactory { get; protected set; }
		public IAssetImporterFactory ImporterFactory { get; protected set; }
		protected Dictionary<uint, string> CommonStringDictionary { get; set; }

		public string GetCommonString(uint index)
		{
			if (CommonStringDictionary?.TryGetValue(index, out string result) ?? false)
				return result;
			else
				return null;
		}

		public string GetClassName(int idNumber)
		{
			return Enum.GetName(ClassIDTypeEnum, idNumber);
		}

		public int GetIdNumber(string className)
		{
			if (Enum.TryParse(ClassIDTypeEnum, className, out var result))
				return (int)result;
			else
				return -1;
		}

		public int[] GetAllValidIdNumbers()
		{
			return (int[])Enum.GetValues(ClassIDTypeEnum);
		}
	}
}
