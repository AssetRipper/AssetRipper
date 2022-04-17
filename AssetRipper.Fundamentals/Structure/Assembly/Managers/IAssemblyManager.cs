using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using Mono.Cecil;
using System;
using System.IO;

namespace AssetRipper.Core.Structure.Assembly.Managers
{
	public interface IAssemblyManager : IDisposable
	{
		void Initialize(PlatformGameStructure gameStructure);
		void Load(string filePath);
		void Read(Stream stream, string fileName);
		void Unload(string fileName);

		bool IsAssemblyLoaded(string assembly);
		bool IsPresent(ScriptIdentifier scriptID);
		bool IsValid(ScriptIdentifier scriptID);
		SerializableType GetSerializableType(ScriptIdentifier scriptID);
		TypeDefinition GetTypeDefinition(ScriptIdentifier scriptID);
		AssemblyDefinition[] GetAssemblies();
		ScriptIdentifier GetScriptID(string assembly, string name);
		ScriptIdentifier GetScriptID(string assembly, string @namespace, string name);

		bool IsSet { get; }
	}
}
