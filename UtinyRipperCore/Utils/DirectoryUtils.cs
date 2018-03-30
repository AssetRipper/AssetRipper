using System;
using System.IO;
using System.Linq;

namespace UtinyRipper
{
	public static class DirectoryUtils
	{
		public static string GetMaxIndexName(string dirPath, string fileName)
		{
			if (!Directory.Exists(dirPath))
			{
				return fileName;
			}

			string[] files = Directory.GetFiles(dirPath, $"{fileName}*.*").Where(t => !t.EndsWith(".meta")).Select(t => Path.GetFileName(t).ToLower()).ToArray();
			if (files.Length == 0)
			{
				return fileName;
			}

			for (int i = 1; i < int.MaxValue; i++)
			{
				string newName = $"{fileName}_{i}.".ToLower();
				if (!files.Any(t => t.StartsWith(newName)))
				{
					return $"{fileName}_{i}";
				}
			}
			throw new Exception($"Can't generate unique name for file {fileName} in directory {dirPath}");
		}
	}
}
