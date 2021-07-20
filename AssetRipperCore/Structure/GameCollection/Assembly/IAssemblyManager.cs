using AssetRipper.Converters.Project.Exporter.Script;
using AssetRipper.Converters.Project.Exporter.Script.Elements;
using AssetRipper.Structure.GameCollection.Assembly.Serializable;
using System;
using System.IO;

namespace AssetRipper.Structure.GameCollection.Assembly
{
	public interface IAssemblyManager : IDisposable
	{
		void Load(string filePath);
		void Read(Stream stream, string fileName);
		void Unload(string fileName);

		bool IsAssemblyLoaded(string assembly);
		bool IsPresent(ScriptIdentifier scriptID);
		bool IsValid(ScriptIdentifier scriptID);
		SerializableType GetSerializableType(ScriptIdentifier scriptID);
		ScriptExportType GetExportType(ScriptExportManager exportManager, ScriptIdentifier scriptID);
		ScriptIdentifier GetScriptID(string assembly, string name);
		ScriptIdentifier GetScriptID(string assembly, string @namespace, string name);

		bool IsSet { get; }
	}
}
