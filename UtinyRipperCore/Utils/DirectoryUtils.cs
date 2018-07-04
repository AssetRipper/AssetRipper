using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

			string escapeFileName = Regex.Escape(fileName);
			Regex regex = new Regex($"{escapeFileName}[_]?[\\d]*\\.[^.]+$");
			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			List<string> files = new List<string>();
			foreach(FileInfo fileInfo in dirInfo.EnumerateFiles())
			{
				if(regex.IsMatch(fileInfo.Name))
				{
					files.Add(fileInfo.Name.ToLower());
				}
			}
			if (files.Count == 0)
			{
				return fileName;
			}

			for (int i = 1; i < int.MaxValue; i++)
			{
				string newName = $"{fileName}_{i}.".ToLower();
				if (files.All(t => !t.StartsWith(newName)))
				{
					return $"{fileName}_{i}";
				}
			}
			throw new Exception($"Can't generate unique name for file {fileName} in directory {dirPath}");
		}
	}
}
