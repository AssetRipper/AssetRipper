using AsmResolver.DotNet.Builder;
using System.IO;

namespace AssemblyDumper.Passes
{
	public static class Pass999_SaveAssembly
	{
		public static void DoPass(DirectoryInfo outputDirectory)
		{
			Console.WriteLine("Pass 999: Save Assembly");
			AssemblyDefinition? assembly = SharedState.Assembly;

			if (!outputDirectory.Exists) Directory.CreateDirectory(outputDirectory.FullName);

			string filePath = Path.Combine(outputDirectory.FullName, SharedState.Assembly.Name!.ToString() + ".dll");

			DotNetDirectoryFactory factory = new DotNetDirectoryFactory();
			//factory.MetadataBuilderFlags |= MetadataBuilderFlags.NoStringsStreamOptimization; //Check later, but currently less than 1% difference
			ManagedPEImageBuilder builder = new ManagedPEImageBuilder(factory);

			Console.WriteLine($"Saving assembly to {filePath}");
			try
			{
				if(File.Exists(filePath))
					File.Delete(filePath);
				assembly.Write(filePath, builder);
			}
			catch(AggregateException aggregateException)
			{
				Console.WriteLine("AggregateException thrown");
				aggregateException = aggregateException.Flatten();
				
				foreach(Exception? error in aggregateException.InnerExceptions)
				{
					Console.WriteLine();
					Console.WriteLine(error.ToString());
					Console.WriteLine();
				}
			}
		}
	}
}