﻿using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Core.Utils;
using AssetRipper.Library;
using System;
using System.IO;

namespace UnitTester
{
	static class Program
	{
		private static GameStructure GameStructure { get; set; }
		private const string TestsDirectory = "../../Tests";

		static void Main(string[] args)
		{
			if (args.Length != 0)
			{
				Console.WriteLine("This program does not accept command line arguments.");
				Console.ReadLine();
				return;
			}

			Logger.Add(new ConsoleLogger(true));
			Logger.Add(new FileLogger("UnitTester.log"));
			Logger.LogSystemInformation("Unit Tester");
			Logger.BlankLine();

			RunTests();
			Console.ReadLine();
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
			foreach (string versionPath in Directory.GetDirectories(TestsDirectory))
			{
				string versionName = Path.GetRelativePath(TestsDirectory, versionPath);
				foreach(string testPath in Directory.GetDirectories(versionPath))
				{
					string testName = Path.GetRelativePath(versionPath, testPath);
					Logger.Info(LogCategory.General, $"Found test: '{testName}' for Unity version: '{versionName}'");
					numTests++;
					string inputPath = Path.Combine(testPath, "Input");
					if (!Directory.Exists(inputPath))
					{
						Logger.Log(LogType.Error, LogCategory.General, $"No input folder for '{testName}' on Unity version '{versionName}'");
					}
					else
					{
						try
						{
							string[] inputFiles = Directory.GetFiles(inputPath);
							string[] inputDirectories = Directory.GetDirectories(inputPath);
							string[] inputPaths = ArrayUtils.Combine(inputFiles, inputDirectories);
							string outputPath = Path.Combine(testPath, "Output");

							Ripper ripper = new Ripper();
							GameStructure gameStructure = ripper.Load(inputPaths);
							PrepareExportDirectory(outputPath);
							ripper.ExportProject(outputPath);
							Logger.Info(LogCategory.General, $"Completed test: '{testName}' for Unity version: '{versionName}'");
							Logger.BlankLine(2);
							numSuccessful++;
						}
						catch (Exception ex)
						{
							Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
							Logger.BlankLine(2);
						}
					}
				}
			}

			Logger.Info(LogCategory.General, $"{numSuccessful}/{numTests} tests successfully completed");
		}

		private static void PrepareExportDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				Logger.Info(LogCategory.Export, "Clearing export directory...");
				Directory.Delete(path, true);
			}
		}
	}
}
