using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Managers;

public interface IAssemblyManager : IDisposable
{
	void Initialize(PlatformGameStructure gameStructure);
	void Load(string filePath, FileSystem fileSystem);
	void Add(AssemblyDefinition assembly);
	void Read(Stream stream, string fileName);
	void Unload(string fileName);

	bool IsAssemblyLoaded(string assembly);
	bool IsPresent(ScriptIdentifier scriptID);
	bool IsValid(ScriptIdentifier scriptID);
	bool TryGetSerializableType(
		ScriptIdentifier scriptID,
		UnityVersion version,
		[NotNullWhen(true)] out SerializableType? scriptType,
		[NotNullWhen(false)] out string? failureReason);
	TypeDefinition GetTypeDefinition(ScriptIdentifier scriptID);
	IEnumerable<AssemblyDefinition> GetAssemblies();
	ScriptIdentifier GetScriptID(string assembly, string @namespace, string name);
	Stream GetStreamForAssembly(AssemblyDefinition assembly);
	void ClearStreamCache();

	bool IsSet { get; }
	ScriptingBackend ScriptingBackend { get; }

	public sealed AssemblyDefinition? Mscorlib => GetAssemblies().FirstOrDefault(a => a.Name == "mscorlib");
	public sealed bool HasMscorlib2 => Mscorlib?.Version.Major == 2;
}
public static class AssemblyManagerExtensions
{
	public static void SaveAssembly(this IAssemblyManager manager, AssemblyDefinition assembly, string path, FileSystem fileSystem)
	{
		using Stream writeStream = fileSystem.File.Create(path);
		manager.SaveAssembly(assembly, writeStream);
	}
	public static void SaveAssembly(this IAssemblyManager manager, AssemblyDefinition assembly, Stream writeStream)
	{
		Stream readStream = manager.GetStreamForAssembly(assembly);
		readStream.Position = 0;
		readStream.CopyTo(writeStream);
	}
}
