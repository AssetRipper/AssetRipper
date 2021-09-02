using AssetRipper.Core.Logging;
using System;

namespace AssetAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("No arguments");
			}
			else
			{
				Logger.Add(new ConsoleLogger(true));
				Analyzer.LoadFiles(args);
			}
			Console.ReadKey();
			return;
		}
	}
}
