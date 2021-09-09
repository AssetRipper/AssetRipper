using System;
using System.IO;

namespace AssetRipper.Core.Utils
{
	public static class TempFolderManager
	{
		private const int NumberOfRandomCharacters = 10;
		public static string TempFolderPath { get; }

		static TempFolderManager()
		{
			TempFolderPath = ExecutingDirectory.Combine("temp");
			DeleteTempFolder();
			DirectoryUtils.CreateDirectory(TempFolderPath);
		}

		private static void DeleteTempFolder()
		{
			if (DirectoryUtils.Exists(TempFolderPath))
				DirectoryUtils.Delete(TempFolderPath, true);
		}

		private static string GetNewRandomTempFolder() => Path.Combine(TempFolderPath, GetRandomString());

		private static string GetRandomString() => GuidUtils.GetNewGuidString(NumberOfRandomCharacters);

		public static string CreateNewRandomTempFolder()
		{
			string path = GetNewRandomTempFolder();
			Directory.CreateDirectory(path);
			return path;
		}

		/// <summary>
		/// Make a temporary file
		/// </summary>
		/// <param name="data"></param>
		/// <param name="fileExtention">The file extension with the dot</param>
		/// <returns>The path to the file</returns>
		public static string WriteToTempFile(byte[] data, string fileExtention)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			string fileName = GetRandomString() + (fileExtention ?? "");
			string filePath = Path.Combine(TempFolderPath, fileName);
			File.WriteAllBytes(filePath, data);
			return filePath;
		}
	}
}
