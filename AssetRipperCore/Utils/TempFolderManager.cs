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
			Directory.CreateDirectory(TempFolderPath);
		}

		private static void DeleteTempFolder()
		{
			if (Directory.Exists(TempFolderPath))
				Directory.Delete(TempFolderPath, true);
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
		/// <param name="fileExtension">The file extension with the dot</param>
		/// <returns>The path to the file</returns>
		public static string WriteToTempFile(byte[] data, string fileExtension)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			string fileName = GetRandomString() + (fileExtension ?? "");
			string filePath = Path.Combine(TempFolderPath, fileName);
			File.WriteAllBytes(filePath, data);
			return filePath;
		}
	}
}
