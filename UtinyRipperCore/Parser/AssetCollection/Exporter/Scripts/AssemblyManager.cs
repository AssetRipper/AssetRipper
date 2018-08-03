using System;
using System.IO;
using UtinyRipper.AssetExporters.Mono;

namespace UtinyRipper.AssetExporters
{
	public class AssemblyManager : IAssemblyManager
	{
		public AssemblyManager(Action<string> requestAssemblyCallback)
		{
			if (requestAssemblyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestAssemblyCallback));
			}
			m_requestAssemblyCallback = requestAssemblyCallback;
		}

		public static bool IsAssembly(string fileName)
		{
			if(MonoManager.IsMonoAssembly(fileName))
			{
				return true;
			}
			return false;
		}
		
		public bool IsValid(string assembly, string name)
		{
			if (m_manager == null)
			{
				return false;
			}
			return m_manager.IsValid(assembly, name);
		}

		public bool IsValid(string assembly, string @namespace, string name)
		{
			if (m_manager == null)
			{
				return false;
			}
			return m_manager.IsValid(assembly, @namespace, name);
		}

		public ScriptStructure CreateStructure(string assembly, string name)
		{
			return m_manager.CreateStructure(assembly, name);
		}

		public ScriptStructure CreateStructure(string assembly, string @namespace, string name)
		{
			return m_manager.CreateStructure(assembly, @namespace, name);
		}

		public void Dispose()
		{
			if(m_manager != null)
			{
				m_manager.Dispose();
				m_manager = null;
			}
		}

		public void Load(string filePath)
		{
			if(m_manager == null)
			{
				CreateManager(filePath);
			}
			m_manager.Load(filePath);
		}

		public void Read(Stream stream, string filePath)
		{
			if (m_manager == null)
			{
				CreateManager(filePath);
			}
			m_manager.Read(stream, filePath);
		}

		public void Unload(string fileName)
		{
			if(m_manager != null)
			{
				m_manager.Unload(fileName);
			}
		}

		private void CreateManager(string assemblyName)
		{
			if(MonoManager.IsMonoAssembly(assemblyName))
			{
				m_manager = new MonoManager(m_requestAssemblyCallback);
				return;
			}
			throw new NotSupportedException();
		}

		private event Action<string> m_requestAssemblyCallback;

		private IAssemblyManager m_manager;
	}
}
