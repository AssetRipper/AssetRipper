using System;
using System.IO;
using uTinyRipper.Converters.Script;
using uTinyRipper.Game.Assembly;

namespace uTinyRipper.Game
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
