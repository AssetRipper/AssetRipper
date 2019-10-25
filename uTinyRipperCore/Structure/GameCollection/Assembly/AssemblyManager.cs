using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly.Mono;
using uTinyRipper.Exporters.Scripts;

namespace uTinyRipper.Assembly
{
	public sealed class AssemblyManager : IAssemblyManager
	{
		public AssemblyManager(Action<string> requestAssemblyCallback)
		{
			if (requestAssemblyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestAssemblyCallback));
			}
			m_requestAssemblyCallback = requestAssemblyCallback;
		}

		~AssemblyManager()
		{
			Dispose(false);
		}

		public static bool IsAssembly(string fileName)
		{
			if (MonoManager.IsMonoAssembly(fileName))
			{
				return true;
			}
			return false;
		}

		public static string ToAssemblyName(string scopeName)
		{
			if (scopeName.EndsWith(MonoManager.AssemblyExtension))
			{
				return scopeName.Substring(0, scopeName.Length - MonoManager.AssemblyExtension.Length);
			}
			return scopeName;
		}

		public void Load(string filePath)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				throw new Exception("You have to set backend first");
			}
			m_manager.Load(filePath);
		}

		public void Read(Stream stream, string fileName)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				throw new Exception("You have to set backend first");
			}
			m_manager.Read(stream, fileName);
		}

		public void Unload(string fileName)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				throw new Exception("You have to set backend first");
			}
			m_manager.Unload(fileName);
		}

		public bool IsAssemblyLoaded(string assembly)
		{
			return m_manager.IsAssemblyLoaded(assembly);
		}

		public bool IsPresent(ScriptIdentifier scriptID)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return false;
			}
			if (scriptID.IsDefault)
			{
				return false;
			}
			return m_manager.IsPresent(scriptID);
		}

		public bool IsValid(ScriptIdentifier scriptID)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return false;
			}
			if (scriptID.IsDefault)
			{
				return false;
			}
			return m_manager.IsValid(scriptID);
		}

		public SerializableType GetSerializableType(ScriptIdentifier scriptID)
		{
			string uniqueName = scriptID.UniqueName;
			if (m_serializableTypes.TryGetValue(uniqueName, out SerializableType type))
			{
				return type;
			}
			return m_manager.GetSerializableType(scriptID);
		}

		public ScriptExportType GetExportType(ScriptExportManager exportManager, ScriptIdentifier scriptID)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				throw new Exception("You have to set backend first");
			}
			return m_manager.GetExportType(exportManager, scriptID);
		}

		public ScriptIdentifier GetScriptID(string assembly, string name)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return default;
			}
			return m_manager.GetScriptID(assembly, name);
		}

		public ScriptIdentifier GetScriptID(string assembly, string @namespace, string name)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return default;
			}
			return m_manager.GetScriptID(assembly, @namespace, name);
		}

		internal void InvokeRequestAssemblyCallback(string assemblyName)
		{
			m_requestAssemblyCallback.Invoke(assemblyName);
		}

		internal void AddSerializableType(string uniqueName, SerializableType scriptType)
		{
			m_serializableTypes.Add(uniqueName, scriptType);
		}

		internal bool TryGetSerializableType(string uniqueName, out SerializableType scriptType)
		{
			return m_serializableTypes.TryGetValue(uniqueName, out scriptType);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (m_manager != null)
			{
				m_manager.Dispose();
			}
		}

		public ScriptingBackEnd ScriptingBackEnd
		{
			get => m_manager == null ? ScriptingBackEnd.Unknown : m_manager.ScriptingBackEnd;
			set
			{
				if (ScriptingBackEnd == value)
				{
					return;
				}

				switch (value)
				{
					case ScriptingBackEnd.Mono:
						m_manager = new MonoManager(this);
						break;

					default:
						throw new NotImplementedException($"Scripting backend {value} is not impelented yet");
				}
			}
		}

		private event Action<string> m_requestAssemblyCallback;

		private IAssemblyManager m_manager;
		private Dictionary<string, SerializableType> m_serializableTypes = new Dictionary<string, SerializableType>();
	}
}
