using System;
using System.IO;

namespace UtinyRipper
{
	public static class PathUtils
	{
		public static string FixResourcePath(string resourcePath)
		{
			const string archivePrefix = "archive:/";
			if (resourcePath.StartsWith(archivePrefix, StringComparison.Ordinal))
			{
				resourcePath = Path.GetFileName(resourcePath);
			}
			return resourcePath;
		}
	}
}
