using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Converters.Script;
using uTinyRipper.Game.Assembly;
using uTinyRipper.Game.Assembly.Mono;
using uTinyRipper.Layout;

namespace uTinyRipper.Game
{
	public sealed class AssemblyManager : IAssemblyManager
	{
		public AssemblyManager(ScriptingBackend backend, AssetLayout layout, Action<string> requestAssemblyCallback)
		{
			m_manager = backend == ScriptingBackend.Mono ? new MonoManager(this) : null;
			Layout = layout;
			m_requestAssemblyCallback = requestAssemblyCallback ?? throw new ArgumentNullException(nameof(requestAssemblyCallback));
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
			if (scopeName.EndsWith(MonoManager.AssemblyExtension, StringComparison.Ordinal))
			{
				return scopeName.Substring(0, scopeName.Length - MonoManager.AssemblyExtension.Length);
			}
			return scopeName;
		}

		public void Load(string filePath)
		{
			m_manager.Load(filePath);
		}

		public void Read(Stream stream, string fileName)
		{
			m_manager.Read(stream, fileName);
		}

		public void Unload(string fileName)
		{
			m_manager.Unload(fileName);
		}

		public bool IsAssemblyLoaded(string assembly)
		{
			return m_manager.IsAssemblyLoaded(assembly);
		}

		public bool IsPresent(ScriptIdentifier scriptID)
		{
			if (!IsSet)
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
			if (!IsSet)
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
			return m_manager.GetExportType(exportManager, scriptID);
		}

		public ScriptIdentifier GetScriptID(string assembly, string name)
		{
			if (!IsSet)
			{
				return default;
			}
			return m_manager.GetScriptID(assembly, name);
		}

		public ScriptIdentifier GetScriptID(string assembly, string @namespace, string name)
		{
			if (!IsSet)
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

		public AssetLayout Layout { get; }
		public bool IsSet => m_manager != null;

		private event Action<string> m_requestAssemblyCallback;

		private readonly Dictionary<string, SerializableType> m_serializableTypes = new Dictionary<string, SerializableType>();
		private IAssemblyManager m_manager;
	}
}
