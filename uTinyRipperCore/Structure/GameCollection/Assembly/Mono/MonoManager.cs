using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Converters.Script;
using uTinyRipper.Layout;

namespace uTinyRipper.Game.Assembly.Mono
{
	internal sealed class MonoManager : IAssemblyManager, IAssemblyResolver
	{
		public MonoManager(AssemblyManager assemblyManager)
		{
			m_assemblyManager = assemblyManager ?? throw new ArgumentNullException(nameof(assemblyManager));
		}

		~MonoManager()
		{
			Dispose(false);
		}

		public static bool IsMonoAssembly(string fileName)
		{
			if (fileName.EndsWith(AssemblyExtension, StringComparison.Ordinal))
			{
				return true;
			}
			return false;
		}

		public static string GetUniqueName(TypeReference type)
		{
			string assembly = FilenameUtils.FixAssemblyEndian(type.Scope.Name);
			return ScriptIdentifier.ToUniqueName(assembly, type.FullName);
		}

		public void Load(string filePath)
		{
			ReaderParameters parameters = new ReaderParameters(ReadingMode.Deferred)
			{
				InMemory = false,
				ReadWrite = false,
				AssemblyResolver = this,
			};
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filePath, parameters);
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			string assemblyName = AssemblyManager.ToAssemblyName(assembly.Name.Name);
			m_assemblies.Add(fileName, assembly);
			m_assemblies[assemblyName] = assembly;
		}

		public void Read(Stream stream, string fileName)
		{
			ReaderParameters parameters = new ReaderParameters(ReadingMode.Immediate)
			{
				InMemory = true,
				ReadWrite = false,
				AssemblyResolver = this,
			};
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(stream, parameters);
			fileName = Path.GetFileNameWithoutExtension(fileName);
			string assemblyName = AssemblyManager.ToAssemblyName(assembly.Name.Name);
			m_assemblies.Add(fileName, assembly);
			m_assemblies[assemblyName] = assembly;
		}

		public void Unload(string fileName)
		{
			if (m_assemblies.TryGetValue(fileName, out AssemblyDefinition assembly))
			{
				assembly.Dispose();
				m_assemblies.Remove(fileName);
			}
		}

		public bool IsAssemblyLoaded(string assembly)
		{
			return m_assemblies.ContainsKey(assembly);
		}

		public bool IsPresent(ScriptIdentifier scriptID)
		{
			return FindType(scriptID.Assembly, scriptID.Namespace, scriptID.Name) != null;
		}

		public bool IsValid(ScriptIdentifier scriptID)
		{
			TypeDefinition type = FindType(scriptID);
			if (type == null)
			{
				return false;
			}
			if (type.IsAbstract)
			{
				return false;
			}
			MonoTypeContext context = new MonoTypeContext(type);
			if (!IsTypeValid(context))
			{
				return false;
			}
			if (!IsInheritanceValid(type))
			{
				return false;
			}
			return true;
		}

		public SerializableType GetSerializableType(ScriptIdentifier scriptID)
		{
			TypeDefinition type = FindType(scriptID);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {scriptID.UniqueName}");
			}
			return new MonoType(this, type);
		}

		public ScriptExportType GetExportType(ScriptExportManager exportManager, ScriptIdentifier scriptID)
		{
			TypeDefinition type = FindType(scriptID);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {scriptID.UniqueName}");
			}
			return exportManager.RetrieveType(type);
		}

		public ScriptIdentifier GetScriptID(string assembly, string name)
		{
			TypeDefinition type = FindType(assembly, name);
			if (type == null)
			{
				return default;
			}
			return new ScriptIdentifier(type.Scope.Name, type.Namespace, type.Name);
		}

		public ScriptIdentifier GetScriptID(string assembly, string @namespace, string name)
		{
			TypeDefinition type = FindType(assembly, @namespace, name);
			if (type == null)
			{
				return default;
			}
			return new ScriptIdentifier(assembly, type.Namespace, type.Name);
		}

		public AssemblyDefinition Resolve(AssemblyNameReference assemblyReference)
		{
			string assemblyName = AssemblyManager.ToAssemblyName(assemblyReference.Name);
			return FindAssembly(assemblyName);
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			return Resolve(name);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public SerializableType GetSerializableType(MonoTypeContext context)
		{
			if (context.Type.ContainsGenericParameter)
			{
				throw new ArgumentException(nameof(context));
			}
			if (MonoType.IsSerializableArray(context.Type))
			{
				throw new ArgumentException(nameof(context));
			}

			string uniqueName = GetUniqueName(context.Type);
			if (m_assemblyManager.TryGetSerializableType(uniqueName, out SerializableType serializableType))
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
			m_assemblyManager.AddSerializableType(uniqueName, scriptType);
		}

		private void Dispose(bool disposing)
		{
			foreach (AssemblyDefinition assembly in m_assemblies.Values)
			{
				if (assembly != null)
				{
					assembly.Dispose();
				}
			}
		}

		private AssemblyDefinition FindAssembly(string name)
		{
			if (m_assemblies.TryGetValue(name, out AssemblyDefinition assembly))
			{
				return assembly;
			}

			m_assemblyManager.InvokeRequestAssemblyCallback(name);
			if (m_assemblies.TryGetValue(name, out assembly))
			{
				return assembly;
			}
			m_assemblies.Add(name, null);
			return null;
		}

		private TypeDefinition FindType(string assembly, string name)
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

		private TypeDefinition FindType(string assembly, string @namespace, string name)
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

		private TypeDefinition FindType(ScriptIdentifier scriptID)
		{
			return FindType(scriptID.Assembly, scriptID.Namespace, scriptID.Name);
		}

		/// <summary>
		/// Is it possible to properly restore serializable layout for specified type
		/// </summary>
		/// <param name="type">Type to check</param>
		/// <param name="arguments">Generic arguments for checking type</param>
		/// <returns>Is type valid for serialization</returns>
		private bool IsTypeValid(MonoTypeContext context)
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
			if (MonoType.IsBuiltinGeneric(context.Type))
			{
				GenericInstanceType generic = (GenericInstanceType)context.Type;
				TypeReference element = generic.GenericArguments[0];
				MonoTypeContext genericContext = new MonoTypeContext(element, context);
				return IsTypeValid(genericContext);
			}

			if (MonoType.IsPrime(context.Type))
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
				if (!MonoType.IsSerializableModifier(field))
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

		private bool IsInheritanceValid(TypeReference type)
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

				if (MonoType.IsMonoBehaviour(definition) || MonoType.IsScriptableObject(definition))
				{
					return true;
				}
				type = definition.BaseType;
			}
			return false;
		}

		public AssetLayout Layout => m_assemblyManager.Layout;
		public bool IsSet => true;

		public const string AssemblyExtension = ".dll";

		private readonly Dictionary<string, AssemblyDefinition> m_assemblies = new Dictionary<string, AssemblyDefinition>();
		private readonly Dictionary<string, bool> m_validTypes = new Dictionary<string, bool>();
		private readonly AssemblyManager m_assemblyManager;
	}
}
