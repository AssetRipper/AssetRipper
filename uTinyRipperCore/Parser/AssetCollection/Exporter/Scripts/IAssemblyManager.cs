using System;
using System.IO;
using uTinyRipper.Exporters.Scripts;

namespace uTinyRipper.AssetExporters
{
	public interface IAssemblyManager : IDisposable
	{
		void Load(string filePath);
		void Read(Stream stream, string fileName);
		void Unload(string fileName);

		bool IsAssemblyLoaded(string assembly);
		bool IsPresent(string assembly, string name);
		bool IsPresent(string assembly, string @namespace, string name);
		bool IsValid(string assembly, string name);
		bool IsValid(string assembly, string @namespace, string name);
		ScriptStructure CreateStructure(string assembly, string name);
		ScriptStructure CreateStructure(string assembly, string @namespace, string name);
		ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string name);
		ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string @namespace, string name);
		ScriptInfo GetScriptInfo(string assembly, string name);

		ScriptingBackEnd ScriptingBackEnd { get; set; }
	}
}
