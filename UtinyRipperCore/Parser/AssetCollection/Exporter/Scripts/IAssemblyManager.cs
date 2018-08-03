using System;
using System.IO;

namespace UtinyRipper.AssetExporters
{
	public interface IAssemblyManager : IDisposable
	{
		void Load(string filePath);
		void Read(Stream stream, string filePath);
		void Unload(string fileName);

		bool IsValid(string assembly, string name);
		bool IsValid(string assembly, string @namespace, string name);
		ScriptStructure CreateStructure(string assembly, string name);
		ScriptStructure CreateStructure(string assembly, string @namespace, string name);
	}
}
