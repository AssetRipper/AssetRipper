using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure.Platforms;

namespace AssetRipper.Core.Structure.Assembly.Managers
{
	public sealed class MonoManager : BaseManager
	{
		public const string AssemblyExtension = ".dll";

		public override ScriptingBackend ScriptingBackend => ScriptingBackend.Mono;

		public MonoManager(LayoutInfo layout, Action<string> requestAssemblyCallback) : base(layout, requestAssemblyCallback) { }

		public override void Initialize(PlatformGameStructure gameStructure)
		{
			Logger.Info(LogCategory.Import, $"During Mono initialization, found {gameStructure.Assemblies.Count} assemblies");
			foreach ((string assemblyName, string assemblyPath) in gameStructure.Assemblies)
			{
				try
				{
					if (!PEFile.FromFile(assemblyPath).OptionalHeader.GetDataDirectory(DataDirectoryIndex.ClrDirectory).IsPresentInPE)
					{
						Logger.Info(LogCategory.Import, $"Skipping native assembly: {assemblyName}");
					}
					else
					{
						Load(assemblyPath);
					}
				}
				catch (BadImageFormatException)
				{
					Logger.Info(LogCategory.Import, $"Skipping non-PE file: {assemblyName}");
				}
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
