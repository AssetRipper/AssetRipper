using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using System;

namespace CodeTester
{
	static class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("This program takes exactly one command line argument.");
				Console.ReadLine();
				return;
			}

			try
			{
				Logger.Add(new ConsoleLogger(true));
				Logger.Add(new FileLogger(ExecutingDirectory.Combine("CodeTester.log")));
				Logger.LogSystemInformation("Code Tester");
				Logger.BlankLine();
				AssetRipper.Library.Ripper ripper = new();
				AssetRipper.Library.Utils.DumpInfo.DumpAllFileInfo(args[0], ExecutingDirectory.Combine("FileInfoDump"));
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}
