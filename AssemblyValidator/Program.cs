using CommandLine;

namespace AssemblyValidator
{
	public static class Program
	{
		private static void Run(Options options)
		{
			Console.WriteLine("Making a new dll");
#if DEBUG
			try
			{
#endif
				Loader.LoadHandlers(options.InputDirectory!.FullName);
				Console.WriteLine("Done!");
#if DEBUG
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
#endif
		}

		internal class Options
		{
			[Value(0, Required = true, HelpText = "Directory with the assemblies for validation")]
			public DirectoryInfo? InputDirectory { get; set; }
		}

		public static void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<Options>(args)
				.WithParsed(options =>
				{
					if (ValidateOptions(options))
					{
						Run(options);
					}
					else
					{
						Environment.ExitCode = 1;
					}
				});
		}

		private static bool ValidateOptions(Options options)
		{
			try
			{
				if (options.InputDirectory == null)
					return false;

				return options.InputDirectory.Exists;
			}
			catch (Exception ex)
			{
				System.Console.WriteLine($"Failed to initialize the paths.");
				System.Console.WriteLine(ex.ToString());
				return false;
			}
		}
	}
}