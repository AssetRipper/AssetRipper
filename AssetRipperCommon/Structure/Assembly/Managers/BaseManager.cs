using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure.Assembly.Mono;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.Structure.Assembly.Managers
{
	public class BaseManager : IAssemblyManager, IAssemblyResolver
	{
		public virtual bool IsSet => false;

		protected readonly Dictionary<string, AssemblyDefinition> m_assemblies = new Dictionary<string, AssemblyDefinition>();
		protected readonly Dictionary<string, bool> m_validTypes = new Dictionary<string, bool>();

		public LayoutInfo Layout { get; }

		private event Action<string> m_requestAssemblyCallback;
		private readonly Dictionary<string, SerializableType> m_serializableTypes = new Dictionary<string, SerializableType>();

		public BaseManager(LayoutInfo layout, Action<string> requestAssemblyCallback)
		{
			Layout = layout;
			m_requestAssemblyCallback = requestAssemblyCallback ?? throw new ArgumentNullException(nameof(requestAssemblyCallback));
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

		protected static string GetUniqueName(TypeReference type)
		{
			string assembly = FilenameUtils.FixAssemblyEndian(type.Scope.Name);
			return ScriptIdentifier.ToUniqueName(assembly, type.FullName);
		}

		public virtual void Load(string filePath)
		{
			ReaderParameters parameters = new ReaderParameters(ReadingMode.Deferred)
			{
				InMemory = false,
				ReadWrite = false,
				AssemblyResolver = this,
			};
			AssemblyDefinition assembly;
			try
			{
				assembly = AssemblyDefinition.ReadAssembly(filePath, parameters);
			}
			catch(BadImageFormatException badImageFormatException)
			{
				throw new BadImageFormatException($"Could not read {filePath}", badImageFormatException);
			}
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			string assemblyName = ToAssemblyName(assembly.Name.Name);
			m_assemblies.Add(fileName, assembly);
			m_assemblies[assemblyName] = assembly;
		}

		public virtual void Read(Stream stream, string fileName)
		{
			ReaderParameters parameters = new ReaderParameters(ReadingMode.Immediate)
			{
				InMemory = true,
				ReadWrite = false,
				AssemblyResolver = this,
			};
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(stream, parameters);
			fileName = Path.GetFileNameWithoutExtension(fileName);
			string assemblyName = ToAssemblyName(assembly.Name.Name);
			m_assemblies.Add(fileName, assembly);
			m_assemblies[assemblyName] = assembly;
		}

		public virtual void Unload(string fileName)
		{
			if (m_assemblies.TryGetValue(fileName, out AssemblyDefinition assembly))
			{
				assembly.Dispose();
				m_assemblies.Remove(fileName);
			}
		}

		public virtual bool IsAssemblyLoaded(string assembly)
		{
			return m_assemblies.ContainsKey(assembly);
		}

		public virtual bool IsPresent(ScriptIdentifier scriptID)
		{
			if (!IsSet)
				return false;
			if (scriptID.IsDefault)
				return false;
			else
				return FindType(scriptID.Assembly, scriptID.Namespace, scriptID.Name) != null;
		}

		public virtual bool IsValid(ScriptIdentifier scriptID)
		{
			if (!IsSet)
				return false;
			if (scriptID.IsDefault)
				return false;
			TypeDefinition type = FindType(scriptID);
			if (type == null)
				return false;
			if (type.IsAbstract)
				return false;

			MonoTypeContext context = new MonoTypeContext(type);
			if (!IsTypeValid(context))
				return false;

			if (!IsInheritanceValid(type))
				return false;

			return true;
		}

		public virtual TypeDefinition GetTypeDefinition(ScriptIdentifier scriptID)
		{
			TypeDefinition type = FindType(scriptID);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {scriptID.UniqueName}");
			}
			return type;
		}

		public virtual ScriptIdentifier GetScriptID(string assembly, string name)
		{
			if (!IsSet)
				return default;
			TypeDefinition type = FindType(assembly, name);
			if (type == null)
			{
				return default;
			}
			return new ScriptIdentifier(type.Scope.Name, type.Namespace, type.Name);
		}

		public virtual ScriptIdentifier GetScriptID(string assembly, string @namespace, string name)
		{
			if (!IsSet)
				return default;
			TypeDefinition type = FindType(assembly, @namespace, name);
			if (type == null)
			{
				return default;
			}
			return new ScriptIdentifier(assembly, type.Namespace, type.Name);
		}

		public virtual AssemblyDefinition Resolve(AssemblyNameReference assemblyReference)
		{
			string assemblyName = ToAssemblyName(assemblyReference.Name);
			return FindAssembly(assemblyName);
		}

		public virtual AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			return Resolve(name);
		}

		public virtual SerializableType GetSerializableType(ScriptIdentifier scriptID)
		{
			string uniqueName = scriptID.UniqueName;
			if (m_serializableTypes.TryGetValue(uniqueName, out SerializableType sType))
			{
				return sType;
			}
			TypeDefinition type = FindType(scriptID);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {scriptID.UniqueName}");
			}
			return new MonoType(this, type);
		}

		public SerializableType GetSerializableType(MonoTypeContext context)
		{
			if (context.Type.ContainsGenericParameter)
			{
				throw new ArgumentException(nameof(context));
			}
			if (MonoUtils.IsSerializableArray(context.Type))
			{
				throw new ArgumentException(nameof(context));
			}

			string uniqueName = GetUniqueName(context.Type);
			if (TryGetSerializableType(uniqueName, out SerializableType serializableType))
			{
				return serializableType;
			}
			else
			{
				return new MonoType(this, context);
			}
		}

		internal void AddSerializableType(TypeReference type, SerializableType scriptType)
		{
			string uniqueName = GetUniqueName(type);
			AddSerializableType(uniqueName, scriptType);
		}

		internal void InvokeRequestAssemblyCallback(string assemblyName) => m_requestAssemblyCallback.Invoke(assemblyName);

		internal void AddSerializableType(string uniqueName, SerializableType scriptType) => m_serializableTypes.Add(uniqueName, scriptType);

		internal bool TryGetSerializableType(string uniqueName, out SerializableType scriptType) => m_serializableTypes.TryGetValue(uniqueName, out scriptType);

		protected AssemblyDefinition FindAssembly(string name)
		{
			if (m_assemblies.TryGetValue(name, out AssemblyDefinition assembly))
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

		protected TypeDefinition FindType(string assembly, string name)
		{
			AssemblyDefinition definition = FindAssembly(assembly);
			if (definition == null)
			{
				return null;
			}

			foreach (ModuleDefinition module in definition.Modules)
			{
				foreach (TypeDefinition type in module.Types)
				{
					if (type.Name == name)
					{
						return type;
					}
				}
			}
			return null;
		}

		protected TypeDefinition FindType(string assembly, string @namespace, string name)
		{
			AssemblyDefinition definition = FindAssembly(assembly);
			if (definition == null)
			{
				return null;
			}

			foreach (ModuleDefinition module in definition.Modules)
			{
				TypeDefinition type = module.GetType(@namespace, name);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		protected TypeDefinition FindType(ScriptIdentifier scriptID)
		{
			return FindType(scriptID.Assembly, scriptID.Namespace, scriptID.Name);
		}

		/// <summary>
		/// Is it possible to properly restore serializable layout for specified type
		/// </summary>
		/// <param name="type">Type to check</param>
		/// <param name="arguments">Generic arguments for checking type</param>
		/// <returns>Is type valid for serialization</returns>
		protected bool IsTypeValid(MonoTypeContext context)
		{
			if (context.Type.IsGenericParameter)
			{
				MonoTypeContext parameterContext = context.Resolve();
				return IsTypeValid(parameterContext);
			}
			if (context.Type.IsArray)
			{
				ArrayType array = (ArrayType)context.Type;
				MonoTypeContext arrayContext = new MonoTypeContext(array.ElementType, context);
				return IsTypeValid(arrayContext);
			}
			if (MonoUtils.IsBuiltinGeneric(context.Type))
			{
				GenericInstanceType generic = (GenericInstanceType)context.Type;
				TypeReference element = generic.GenericArguments[0];
				MonoTypeContext genericContext = new MonoTypeContext(element, context);
				return IsTypeValid(genericContext);
			}

			if (MonoUtils.IsPrime(context.Type))
			{
				return true;
			}
			if (context.Type.Module == null)
			{
				return false;
			}

			if (m_validTypes.TryGetValue(context.Type.FullName, out bool isValid))
			{
				return isValid;
			}

			// set value right here to prevent recursion
			m_validTypes[context.Type.FullName] = true;

			// Resolve method for generic instance returns template definition, so we need to check module for template first
			if (context.Type.IsGenericInstance)
			{
				GenericInstanceType instance = (GenericInstanceType)context.Type;
				if (instance.ElementType.Module == null)
				{
					m_validTypes[context.Type.FullName] = false;
					return false;
				}
			}

			TypeDefinition definition = context.Type.Resolve();
			if (definition == null)
			{
				m_validTypes[context.Type.FullName] = false;
				return false;
			}
			if (definition.IsInterface)
			{
				return true;
			}

			MonoTypeContext baseContext = context.GetBase();
			if (!IsTypeValid(baseContext))
			{
				m_validTypes[context.Type.FullName] = false;
				return false;
			}

			IReadOnlyDictionary<GenericParameter, TypeReference> arguments = context.GetContextArguments();
			foreach (FieldDefinition field in definition.Fields)
			{
				if (!MonoUtils.IsSerializableModifier(field))
				{
					continue;
				}

				MonoTypeContext fieldContext = new MonoTypeContext(field.FieldType, arguments);
				if (!IsTypeValid(fieldContext))
				{
					m_validTypes[context.Type.FullName] = false;
					return false;
				}
			}
			return true;
		}

		protected bool IsInheritanceValid(TypeReference type)
		{
			while (type != null)
			{
				if (type.Module == null)
				{
					return false;
				}
				TypeDefinition definition = type.Resolve();
				if (definition == null)
				{
					return false;
				}

				if (MonoUtils.IsMonoBehaviour(definition) || MonoUtils.IsScriptableObject(definition))
				{
					return true;
				}
				type = definition.BaseType;
			}
			return false;
		}

		public virtual AssemblyDefinition[] GetAssemblies()
		{
			return m_assemblies.Values.Where(x => x != null).Distinct().ToArray();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			foreach (AssemblyDefinition assembly in m_assemblies.Values)
			{
				if (assembly != null)
				{
					assembly.Dispose();
				}
			}
		}

		~BaseManager()
		{
			Dispose(false);
		}

	}
}
