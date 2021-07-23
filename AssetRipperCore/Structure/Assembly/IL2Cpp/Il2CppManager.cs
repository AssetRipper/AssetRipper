using AssetRipper.Converters.Project.Exporters.Script;
using AssetRipper.Converters.Project.Exporters.Script.Elements;
using AssetRipper.Layout;
using AssetRipper.Logging;
using AssetRipper.Parser.Utils;
using AssetRipper.Structure.Assembly.Serializable;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Structure.Assembly.Il2Cpp
{
	internal sealed class Il2CppManager : IAssemblyManager, IAssemblyResolver
	{
		private AssemblyManager m_assemblyManager;
		private bool disposedValue;
		public const string GameAssemblyName = "GameAssembly.dll";

		public Il2CppManager(AssemblyManager assemblyManager)
		{
			m_assemblyManager = assemblyManager ?? throw new ArgumentNullException(nameof(assemblyManager));
		}

		~Il2CppManager()
		{
			Dispose(false);
		}

		public bool IsSet => throw new NotImplementedException();

		public ScriptExportType GetExportType(ScriptExportManager exportManager, ScriptIdentifier scriptID)
		{
			throw new NotImplementedException();
		}

		public ScriptIdentifier GetScriptID(string assembly, string name)
		{
			throw new NotImplementedException();
		}

		public ScriptIdentifier GetScriptID(string assembly, string @namespace, string name)
		{
			throw new NotImplementedException();
		}

		public SerializableType GetSerializableType(ScriptIdentifier scriptID)
		{
			throw new NotImplementedException();
		}

		public bool IsAssemblyLoaded(string assembly)
		{
			throw new NotImplementedException();
		}

		public bool IsPresent(ScriptIdentifier scriptID)
		{
			throw new NotImplementedException();
		}

		public bool IsValid(ScriptIdentifier scriptID)
		{
			throw new NotImplementedException();
		}

		public void Load(string filePath)
		{
			throw new NotImplementedException();
		}

		public void Read(Stream stream, string fileName)
		{
			throw new NotImplementedException();
		}

		public void Unload(string fileName)
		{
			throw new NotImplementedException();
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~Il2CppManager()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			throw new NotImplementedException();
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			throw new NotImplementedException();
		}

		public static bool IsIl2Cpp(string[] assemblyNames)
		{
			if (assemblyNames == null) throw new ArgumentNullException(nameof(assemblyNames));
			foreach (string name in assemblyNames)
			{
				if (name == GameAssemblyName)
					return true;
			}
			return false;
		}

		public static bool IsIl2Cpp(string assemblyName)
		{
			if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));
			else return assemblyName == GameAssemblyName;
		}
	}
}
