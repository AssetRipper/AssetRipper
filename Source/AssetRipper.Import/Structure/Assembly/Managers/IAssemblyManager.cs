using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Managers
{
	public interface IAssemblyManager : IDisposable
	{
		void Initialize(PlatformGameStructure gameStructure);
		void Load(string filePath);
		void Add(AssemblyDefinition assembly);
		void Read(Stream stream, string fileName);
		void Unload(string fileName);

		bool IsAssemblyLoaded(string assembly);
		bool IsPresent(ScriptIdentifier scriptID);
		bool IsValid(ScriptIdentifier scriptID);
		bool TryGetSerializableType(
			ScriptIdentifier scriptID,
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
	}
	public static class AssemblyManagerExtensions
	{
		public static void SaveAssembly(this IAssemblyManager manager, AssemblyDefinition assembly, string path)
		{
			Stream readStream = manager.GetStreamForAssembly(assembly);
			using FileStream writeStream = File.Create(path);
			readStream.Position = 0;
			readStream.CopyTo(writeStream);
		}
		public static void SaveAssembly(this IAssemblyManager manager, AssemblyDefinition assembly, Stream writeStream)
		{
			Stream readStream = manager.GetStreamForAssembly(assembly);
			readStream.Position = 0;
			readStream.CopyTo(writeStream);
		}
	}
}
