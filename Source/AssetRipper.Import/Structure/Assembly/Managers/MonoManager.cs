using AsmResolver.PE.File;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;

namespace AssetRipper.Import.Structure.Assembly.Managers;

public sealed class MonoManager : BaseManager
{
	public const string AssemblyExtension = ".dll";

	public override ScriptingBackend ScriptingBackend => ScriptingBackend.Mono;

	public MonoManager(Action<string> requestAssemblyCallback) : base(requestAssemblyCallback) { }

	public override void Initialize(PlatformGameStructure gameStructure)
	{
		Logger.Info(LogCategory.Import, $"During Mono initialization, found {gameStructure.Assemblies.Count} assemblies");
		foreach ((string assemblyName, string assemblyPath) in gameStructure.Assemblies)
		{
			try
			{
				using Stream stream = gameStructure.FileSystem.File.OpenRead(assemblyPath);
				PEFile peFile = PEFile.FromStream(stream);
				if (!peFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ClrDirectory).IsPresentInPE)
				{
					Logger.Info(LogCategory.Import, $"Skipping native assembly: {assemblyName}");
				}
				else
				{
					Load(assemblyPath, gameStructure.FileSystem);
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
