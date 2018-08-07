using System;
using System.IO;
using UtinyRipper.Exporters.Scripts;

namespace UtinyRipper.AssetExporters
{
	public interface IAssemblyManager : IDisposable
	{
		void Load(string filePath);
		void Read(Stream stream, string filePath);
		void Unload(string fileName);

		bool IsPresent(string assembly, string name);
		bool IsPresent(string assembly, string @namespace, string name);
		bool IsValid(string assembly, string name);
		bool IsValid(string assembly, string @namespace, string name);
		ScriptStructure CreateStructure(string assembly, string name);
		ScriptStructure CreateStructure(string assembly, string @namespace, string name);
		ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string name);
		ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string @namespace, string name);

		ScriptingBackEnd ScriptingBackEnd { get; set; }
	}
}
