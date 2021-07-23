using AssetRipper.Logging;
using System;
using System.IO;

namespace AssetRipper.Structure.Assembly.Scripting
{
	internal sealed class MonoManager : BaseManager
	{
		public const string AssemblyExtension = ".dll";
		public string GameDataPath { get; private set; }
		public string ManagedPath { get; private set; }

		public MonoManager(AssemblyManager assemblyManager) : base(assemblyManager) { }

		public override void Initialize(string gameDataPath)
		{
			if (string.IsNullOrWhiteSpace(gameDataPath)) throw new ArgumentNullException(nameof(gameDataPath));

			GameDataPath = Path.GetFullPath(gameDataPath);
			ManagedPath = Path.Combine(GameDataPath, "Managed");

			string[] assemblyFiles = Directory.GetFiles(ManagedPath, "*.dll");

			Logger.Log(LogType.Info, LogCategory.Import, $"During Mono initialization, found {assemblyFiles.Length} assemblies");
			foreach (string assembly in assemblyFiles)
			{
				Load(assembly);
			}
		}

		public static bool IsMonoAssembly(string fileName)
		{
			if (fileName.EndsWith(AssemblyExtension, StringComparison.Ordinal))
			{
				return true;
			}
			return false;
		}
	}
}
