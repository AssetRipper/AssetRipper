﻿using AsmResolver.DotNet;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using AssetRipper.IO.Files.Utils;
using AssetRipper.SerializationLogic;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.Structure.Assembly.Managers
{

	public partial class BaseManager : IAssemblyManager
	{
		public bool IsSet => ScriptingBackend != ScriptingBackend.Unknown;
		public virtual ScriptingBackend ScriptingBackend => ScriptingBackend.Unknown;

		protected readonly Dictionary<string, AssemblyDefinition?> m_assemblies = new();
		protected readonly Dictionary<AssemblyDefinition, Stream> m_assemblyStreams = new();
		protected readonly Dictionary<string, bool> m_validTypes = new();

		private event Action<string> m_requestAssemblyCallback;
		private readonly Dictionary<string, SerializableType> m_serializableTypes = new();
		private readonly Resolver assemblyResolver;
		public IAssemblyResolver AssemblyResolver => assemblyResolver;

		public BaseManager(Action<string> requestAssemblyCallback)
		{
			m_requestAssemblyCallback = requestAssemblyCallback ?? throw new ArgumentNullException(nameof(requestAssemblyCallback));
			assemblyResolver = new Resolver(this);
		}

		public virtual void Initialize(PlatformGameStructure gameStructure) { }

		public static string ToAssemblyName(string scopeName)
		{
			if (scopeName.EndsWith(MonoManager.AssemblyExtension, StringComparison.Ordinal))
			{
				return scopeName.Substring(0, scopeName.Length - MonoManager.AssemblyExtension.Length);
			}
			return scopeName;
		}

		protected static string GetUniqueName(ITypeDefOrRef type)
		{
			string assembly = FilenameUtils.FixAssemblyEndian(type.Scope?.Name ?? "");
			return ScriptIdentifier.ToUniqueName(assembly, type.FullName);
		}

		public virtual void Load(string filePath)
		{
			AssemblyDefinition assembly;
			try
			{
				assembly = AssemblyDefinition.FromFile(filePath);
			}
			catch (BadImageFormatException badImageFormatException)
			{
				throw new BadImageFormatException($"Could not read {filePath}", badImageFormatException);
			}
			assembly.InitializeResolvers(this);
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			string assemblyName = ToAssemblyName(assembly);
			m_assemblies.Add(fileName, assembly);
			m_assemblies[assemblyName] = assembly;
			FileStream stream = File.OpenRead(filePath);
			m_assemblyStreams.Add(assembly, stream);
		}

		public Stream GetStreamForAssembly(AssemblyDefinition assembly)
		{
			if (m_assemblyStreams.TryGetValue(assembly, out Stream? result))
			{
				return result;
			}
			else
			{
				MemoryStream memoryStream = new();
				assembly.WriteManifest(memoryStream);
				m_assemblyStreams.Add(assembly, memoryStream);
				return memoryStream;
			}
		}

		private static string ToAssemblyName(AssemblyDefinition assembly)
		{
			return ToAssemblyName(assembly.Name?.ToString() ?? "");
		}

		public virtual void Read(Stream stream, string fileName)
		{
			MemoryStream memoryStream = new();
			stream.CopyTo(memoryStream);
			AssemblyDefinition assembly = AssemblyDefinition.FromBytes(memoryStream.ToArray());
			//AssemblyDefinition assembly = AssemblyDefinition.FromImage(stream);//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			assembly.InitializeResolvers(this);
			fileName = Path.GetFileNameWithoutExtension(fileName);
			string assemblyName = ToAssemblyName(assembly);
			m_assemblies.Add(fileName, assembly);
			m_assemblies[assemblyName] = assembly;
		}

		public virtual void Unload(string fileName)
		{
			if (m_assemblies.TryGetValue(fileName, out AssemblyDefinition? assembly))
			{
				m_assemblies.Remove(fileName);
				if (assembly is not null && m_assemblyStreams.TryGetValue(assembly, out Stream? stream))
				{
					m_assemblyStreams.Remove(assembly);
					stream.Dispose();
				}
			}
		}

		public virtual bool IsAssemblyLoaded(string assembly)
		{
			return m_assemblies.ContainsKey(assembly);
		}

		public virtual bool IsPresent(ScriptIdentifier scriptID)
		{
			if (!IsSet)
			{
				return false;
			}

			if (scriptID.IsDefault)
			{
				return false;
			}
			else
			{
				return FindType(scriptID.Assembly, scriptID.Namespace, scriptID.Name) != null;
			}
		}

		public virtual bool IsValid(ScriptIdentifier scriptID)
		{
			if (!IsSet)
			{
				return false;
			}

			if (scriptID.IsDefault)
			{
				return false;
			}

			TypeDefinition? type = FindType(scriptID);
			if (type == null)
			{
				return false;
			}

			if (type.IsAbstract)
			{
				return false;
			}

			return FieldSerializationLogic.IsTypeSerializable(type);
		}

		public virtual TypeDefinition GetTypeDefinition(ScriptIdentifier scriptID)
		{
			return FindType(scriptID) ?? throw new ArgumentException($"Can't find type {scriptID.UniqueName}");
		}

		public virtual ScriptIdentifier GetScriptID(string assembly, string name)
		{
			if (!IsSet)
			{
				return default;
			}

			TypeDefinition? type = FindType(assembly, name);
			if (type == null)
			{
				return default;
			}
			return new ScriptIdentifier(type.Module?.Name ?? "", type.Namespace ?? "", type.Name ?? "");
		}

		public virtual ScriptIdentifier GetScriptID(string assembly, string @namespace, string name)
		{
			if (!IsSet)
			{
				return default;
			}

			TypeDefinition? type = FindType(assembly, @namespace, name);
			if (type == null)
			{
				return default;
			}
			return new ScriptIdentifier(assembly, type.Namespace ?? "", type.Name ?? "");
		}

		public virtual SerializableType GetSerializableType(ScriptIdentifier scriptID, UnityVersion version)
		{
			string uniqueName = scriptID.UniqueName;
			if (m_serializableTypes.TryGetValue(uniqueName, out SerializableType? sType))
			{
				return sType;
			}
			TypeDefinition? type = FindType(scriptID);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {scriptID.UniqueName}");
			}
			return new MonoType(type);
		}

		internal void AddSerializableType(ITypeDefOrRef type, SerializableType scriptType)
		{
			string uniqueName = GetUniqueName(type);
			AddSerializableType(uniqueName, scriptType);
		}

		internal void InvokeRequestAssemblyCallback(string assemblyName) => m_requestAssemblyCallback.Invoke(assemblyName);

		internal void AddSerializableType(string uniqueName, SerializableType scriptType) => m_serializableTypes.Add(uniqueName, scriptType);

		internal bool TryGetSerializableType(string uniqueName, [NotNullWhen(true)] out SerializableType? scriptType)
		{
			return m_serializableTypes.TryGetValue(uniqueName, out scriptType);
		}

		protected AssemblyDefinition? FindAssembly(string name)
		{
			if (m_assemblies.TryGetValue(name, out AssemblyDefinition? assembly))
			{
				return assembly;
			}

			InvokeRequestAssemblyCallback(name);
			if (m_assemblies.TryGetValue(name, out assembly))
			{
				return assembly;
			}
			m_assemblies.Add(name, null);
			return null;
		}

		protected TypeDefinition? FindType(string assembly, string name)
		{
			AssemblyDefinition? definition = FindAssembly(assembly);
			if (definition == null)
			{
				return null;
			}

			foreach (ModuleDefinition module in definition.Modules)
			{
				foreach (TypeDefinition type in module.TopLevelTypes)
				{
					if (type.Name == name)
					{
						return type;
					}
				}
			}
			return null;
		}

		protected TypeDefinition? FindType(string assembly, string @namespace, string name)
		{
			AssemblyDefinition? definition = FindAssembly(assembly);
			if (definition == null)
			{
				return null;
			}

			foreach (ModuleDefinition module in definition.Modules)
			{
				TypeDefinition? type = module.GetType(@namespace, name);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		protected TypeDefinition? FindType(ScriptIdentifier scriptID)
		{
			return FindType(scriptID.Assembly, scriptID.Namespace, scriptID.Name);
		}

		public virtual AssemblyDefinition[] GetAssemblies()
		{
			return m_assemblies.Values.Where(x => x is not null).Distinct().ToArray()!;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			
		}

		~BaseManager()
		{
			Dispose(false);
		}

	}
	internal static class ModuleExtensions
	{
		public static TypeDefinition? GetType(this ModuleDefinition module, string @namespace, string name)
		{
			IList<TypeDefinition> types = module.TopLevelTypes;
			foreach (TypeDefinition type in types)
			{
				if (type.Namespace == @namespace && type.Name == name)
				{
					return type;
				}
			}

			return null;
		}
		
		public static void SetResolver(this ModuleDefinition module, IAssemblyResolver assemblyResolver)
		{
			module.MetadataResolver = new DefaultMetadataResolver(assemblyResolver);
		}
		public static void InitializeResolvers(this AssemblyDefinition assembly, BaseManager assemblyManager)
		{
			for (int i = 0; i < assembly.Modules.Count; i++)
			{
				assembly.Modules[i].SetResolver(assemblyManager.AssemblyResolver);
			}
		}
	}
}
