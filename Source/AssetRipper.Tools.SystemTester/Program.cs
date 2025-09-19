using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;
using AssetRipper.Processing;

namespace AssetRipper.Tools.SystemTester;

static class Program
{
	private const string TestsDirectory = "../../Tests";

	static void Main(string[] args)
	{
		Logger.Add(new ConsoleLogger(true));
		Logger.Add(new FileLogger("AssetRipper.Tools.SystemTester.log"));
		Logger.LogSystemInformation("System Tester");
		Logger.BlankLine();

		if (args.Length == 0)
		{
			RunTests();
			Console.ReadLine();
		}
		else
		{
			Rip(args, Path.Join(AppContext.BaseDirectory, "Ripped"));
		}
	}

	static void RunTests()
	{
		if (!Directory.Exists(TestsDirectory))
		{
			Logger.Log(LogType.Warning, LogCategory.General, "Tests folder did not exist. Creating...");
			Directory.CreateDirectory(TestsDirectory);
			Logger.Info(LogCategory.General, "Created. Program will now exit.");
			return;
		}

		Logger.Info(LogCategory.General, $"Running tests in {Path.GetFullPath(TestsDirectory)}");
		Logger.BlankLine();

		int numTests = 0;
		int numSuccessful = 0;
		List<(string, string)> successfulTests = new();
		List<(string, string)> unsuccessfulTests = new();
		foreach (string versionPath in Directory.GetDirectories(TestsDirectory))
		{
			string versionName = Path.GetRelativePath(TestsDirectory, versionPath);
			foreach (string testPath in Directory.GetDirectories(versionPath))
			{
				string testName = Path.GetRelativePath(versionPath, testPath);
				Logger.Info(LogCategory.General, $"Found test: '{testName}' for Unity version: '{versionName}'");
				numTests++;
				string inputPath = Path.Join(testPath, "Input");
				if (!Directory.Exists(inputPath))
				{
					Logger.Log(LogType.Error, LogCategory.General, $"No input folder for '{testName}' on Unity version '{versionName}'");
					unsuccessfulTests.Add((versionName, testName));
				}
				else
				{
					try
					{
						string[] inputFiles = Directory.GetFiles(inputPath);
						string[] inputDirectories = Directory.GetDirectories(inputPath);
						string[] inputPaths = Combine(inputFiles, inputDirectories);
						string outputPath = Path.Join(testPath, "Output");
						Rip(inputPaths, outputPath);
						Logger.Info(LogCategory.General, $"Completed test: '{testName}' for Unity version: '{versionName}'");
						Logger.BlankLine(2);
						numSuccessful++;
						successfulTests.Add((versionName, testName));
					}
					catch (Exception ex)
					{
						Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
						Logger.BlankLine(2);
						unsuccessfulTests.Add((versionName, testName));
					}
				}
			}
		}

		Logger.Info(LogCategory.General, $"{numSuccessful}/{numTests} tests successfully completed");
		if (numSuccessful > 0)
		{
			Logger.Info(LogCategory.General, "Successful:");
			foreach ((string version, string test) in successfulTests)
			{
				Logger.Info(LogCategory.General, $"\t{version,-12} {test}");
			}
		}
		if (numSuccessful < numTests)
		{
			Logger.Info(LogCategory.General, "Unsuccessful:");
			foreach ((string version, string test) in unsuccessfulTests)
			{
				Logger.Info(LogCategory.General, $"\t{version,-12} {test}");
			}
		}
	}

	private static void Rip(string[] inputPaths, string outputPath)
	{
		FullConfiguration settings = new();
		settings.LogConfigurationValues();
		ExportHandler exportHandler = new(settings);
		GameData gameData = exportHandler.LoadAndProcess(inputPaths, LocalFileSystem.Instance);
		PrepareExportDirectory(outputPath);
		exportHandler.Export(gameData, outputPath, LocalFileSystem.Instance);
	}

	private static void PrepareExportDirectory(string path)
	{
		if (Directory.Exists(path))
		{
			Logger.Info(LogCategory.Export, "Clearing export directory...");
			Directory.Delete(path, true);
		}
	}

	private static T[] Combine<T>(T[] array1, T[] array2)
	{
		ArgumentNullException.ThrowIfNull(array1);
		ArgumentNullException.ThrowIfNull(array2);

		T[] result = new T[array1.Length + array2.Length];
		for (int i = 0; i < array1.Length; i++)
		{
			result[i] = array1[i];
		}
		for (int j = 0; j < array2.Length; j++)
		{
			result[j + array1.Length] = array2[j];
		}
		return result;
	}
}
