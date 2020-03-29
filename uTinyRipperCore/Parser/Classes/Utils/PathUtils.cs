using System;
using System.IO;

namespace uTinyRipper.Classes
{
	public static class PathUtils
	{
		public static string SubstituteResourcePath(Object asset, string resourcePath)
		{
			return Path.Combine(ResourceFullPath, SubstituteExportPath(resourcePath, asset.TryGetName()));
		}

		public static string SubstituteAssetBundlePath(Object asset, string assetPath)
		{
			return SubstituteExportPath(assetPath, asset.TryGetName());
		}

		private static string SubstituteExportPath(string assetPath, string assetName)
		{
			if (assetName.Length > 0 && assetName != assetPath && assetPath.EndsWith(assetName, StringComparison.OrdinalIgnoreCase))
			{
				if (assetName.Length == assetPath.Length)
				{
					return assetName;
				}
				if (assetPath[assetPath.Length - assetName.Length - 1] == DirectorySeparator)
				{
					string directoryPath = assetPath.Substring(0, assetPath.Length - assetName.Length);
					return directoryPath + assetName;
				}
			}

			return assetPath;
		}

		private const string ResourceKeyword = "Resources";
		private const char DirectorySeparator = '/';

		private static readonly string ResourceFullPath = Path.Combine(Object.AssetsKeyword, ResourceKeyword);
	}
}
