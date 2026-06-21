using AsmResolver.DotNet;
using AsmResolver.PE.File;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Assembly.Managers;

public sealed class MonoManager : BaseManager
{
	public const string AssemblyExtension = ".dll";

	public override ScriptingBackend ScriptingBackend => ScriptingBackend.Mono;

	public MonoManager(Action<string> requestAssemblyCallback) : base(requestAssemblyCallback)
	{
	}

	public override void Initialize(PlatformGameStructure gameStructure)
	{
		Logger.Info(LogCategory.Import, $"During Mono initialization, found {gameStructure.Assemblies.Count} assemblies");

		Debug.Assert(RuntimeContext is null);

		AssemblyDefinition mscorlib;
		if (TryGetMscorlibPath(gameStructure.Assemblies, out string? mscorlibPath))
		{
			mscorlib = TryLoad("mscorlib.dll", mscorlibPath, gameStructure.FileSystem) ?? LoadSystemRuntimeAsMscorlib();
		}
		else
		{
			mscorlib = LoadSystemRuntimeAsMscorlib();
		}

		// The runtime info is irrelevant because we're creating our own corlib, but AsmResolver still requires that we specify one.
		RuntimeContext runtimeContext = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0), (bool?)null, mscorlib);

		runtimeContext.AddAssembly(mscorlib);
		Debug.Assert(RuntimeContext is not null);

		foreach ((string assemblyName, string assemblyPath) in gameStructure.Assemblies)
		{
			if (assemblyPath != mscorlibPath)
			{
				TryLoad(assemblyName, assemblyPath, gameStructure.FileSystem);
			}
		}

		AssemblyDefinition LoadSystemRuntimeAsMscorlib()
		{
			AssemblyDefinition assembly = AssemblyDefinition.FromBytes(Basic.Reference.Assemblies.Net100.ReferenceInfos.SystemRuntime.ImageBytes, createRuntimeContext: false);
			assembly.Name = "mscorlib";
			assembly.ManifestModule!.Name = "mscorlib.dll";
			assembly.Version = new Version(4, 0, 0, 0);
			Add(assembly);
			return assembly;
		}

		AssemblyDefinition? TryLoad(string assemblyName, string assemblyPath, FileSystem fileSystem)
		{
			bool? isDotNetModule = IsDotNetModule(assemblyPath, fileSystem);
			if (isDotNetModule is null)
			{
				Logger.Info(LogCategory.Import, $"Skipping non-PE file: {assemblyName}");
				return null;
			}
			else if (isDotNetModule is false)
			{
				Logger.Info(LogCategory.Import, $"Skipping native assembly: {assemblyName}");
				return null;
			}
			else
			{
				return Load(assemblyPath, fileSystem);
			}
		}

		static bool TryGetMscorlibPath(Dictionary<string, string> assemblies, [NotNullWhen(true)] out string? mscorlibPath)
		{
			// Fast path
			if (assemblies.TryGetValue("mscorlib.dll", out mscorlibPath))
			{
				return true;
			}

			foreach ((string assemblyName, string assemblyPath) in assemblies)
			{
				if (string.Equals(assemblyName, "mscorlib.dll", StringComparison.OrdinalIgnoreCase))
				{
					mscorlibPath = assemblyPath;
					return true;
				}
			}
			return false;
		}
	}

	private static bool? IsDotNetModule(string path, FileSystem fileSystem)
	{
		try
		{
			using Stream stream = fileSystem.File.OpenRead(path);
			PEFile peFile = PEFile.FromStream(stream);
			return peFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ClrDirectory).IsPresentInPE;
		}
		catch (BadImageFormatException)
		{
			return null;
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
