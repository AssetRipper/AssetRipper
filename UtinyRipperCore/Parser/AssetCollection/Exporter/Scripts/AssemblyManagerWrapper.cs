using System;
using System.IO;
using UtinyRipper.Exporters.Scripts;

namespace UtinyRipper.AssetExporters
{
	public class AssemblyManagerWrapper : IAssemblyManager
	{
		public AssemblyManagerWrapper(IAssemblyManager assemblyManager, Action<string> requestAssemblyCallback)
		{
			if(assemblyManager == null)
			{
				throw new ArgumentNullException(nameof(assemblyManager));
			}
			if (requestAssemblyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestAssemblyCallback));
			}
			m_assemblyManager = assemblyManager;
			m_requestAssemblyCallback = requestAssemblyCallback;
		}

		public void Load(string filePath)
		{
			m_assemblyManager.Load(filePath);
		}

		public void Read(Stream stream, string fileName)
		{
			m_assemblyManager.Read(stream, fileName);
		}

		public void Unload(string fileName)
		{
			m_assemblyManager.Unload(fileName);
		}

		public bool IsAssemblyLoaded(string assembly)
		{
			return m_assemblyManager.IsAssemblyLoaded(assembly);
		}

		public bool IsPresent(string assembly, string name)
		{
			if(!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.IsPresent(assembly, name);
		}

		public bool IsPresent(string assembly, string @namespace, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.IsPresent(assembly, @namespace, name);
		}

		public bool IsValid(string assembly, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.IsValid(assembly, name);
		}

		public bool IsValid(string assembly, string @namespace, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.IsValid(assembly, @namespace, name);
		}

		public ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.CreateExportType(exportManager, assembly, name);
		}

		public ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string @namespace, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.CreateExportType(exportManager, assembly, @namespace, name);
		}

		public ScriptStructure CreateStructure(string assembly, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.CreateStructure(assembly, name);
		}

		public ScriptStructure CreateStructure(string assembly, string @namespace, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.CreateStructure(assembly, @namespace, name);
		}

		public void Dispose()
		{
			m_assemblyManager.Dispose();
		}

		public ScriptInfo GetScriptInfo(string assembly, string name)
		{
			if (!m_assemblyManager.IsAssemblyLoaded(assembly))
			{
				m_requestAssemblyCallback.Invoke(assembly);
			}
			return m_assemblyManager.GetScriptInfo(assembly, name);
		}

		public ScriptingBackEnd ScriptingBackEnd
		{
			get => m_assemblyManager.ScriptingBackEnd;
			set => m_assemblyManager.ScriptingBackEnd = value;
		}

		private readonly IAssemblyManager m_assemblyManager;
		private readonly Action<string> m_requestAssemblyCallback;
	}
}
