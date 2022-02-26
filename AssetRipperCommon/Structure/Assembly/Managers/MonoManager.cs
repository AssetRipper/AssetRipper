using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using System;
using System.IO;

namespace AssetRipper.Core.Structure.Assembly.Managers
{
	public sealed class MonoManager : BaseManager
	{
		public const string AssemblyExtension = ".dll";
		public string GameDataPath { get; private set; }
		public string ManagedPath { get; private set; }

		public override bool IsSet => true;

		public MonoManager(LayoutInfo layout, Action<string> requestAssemblyCallback) : base(layout, requestAssemblyCallback) { }

		public override void Initialize(PlatformGameStructure gameStructure)
		{
			string gameDataPath = gameStructure?.GameDataPath;
			if (string.IsNullOrWhiteSpace(gameDataPath)) return;//Mixed Game Structures don't necessarily have a managed folder

			GameDataPath = Path.GetFullPath(gameDataPath);
			ManagedPath = gameStructure.ManagedPath ?? throw new ArgumentException("Managed Path cannot be null");

			string[] assemblyFiles = Directory.GetFiles(ManagedPath, "*.dll");

			Logger.Info(LogCategory.Import, $"During Mono initialization, found {assemblyFiles.Length} assemblies");
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

		~MonoManager()
		{
			Dispose(false);
		}
	}
}
