using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Exporters.Scripts;

namespace UtinyRipper.AssetExporters.Mono
{
	internal sealed class MonoManager : IAssemblyManager, IAssemblyResolver
	{
		public MonoManager(Action<string> requestAssemblyCallback)
		{
			if(requestAssemblyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestAssemblyCallback));
			}
			m_requestAssemblyCallback = requestAssemblyCallback;
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
			if(m_assemblies.TryGetValue(fileName, out AssemblyDefinition assembly))
			{
				assembly.Dispose();
				m_assemblies.Remove(fileName);
			}
		}

		public bool IsAssemblyLoaded(string assembly)
		{
			return m_assemblies.ContainsKey(assembly);
		}

		public bool IsPresent(string assembly, string name)
		{
			return FindType(assembly, name) != null;
		}

		public bool IsPresent(string assembly, string @namespace, string name)
		{
			return FindType(assembly, @namespace, name) != null;
		}

		public bool IsValid(string assembly, string name)
		{
			TypeDefinition type = FindType(assembly, name);
			if(type == null)
			{
				return false;
			}
			return IsTypeValid(type, s_emptyArguments);
		}

		public bool IsValid(string assembly, string @namespace, string name)
		{
			TypeDefinition type = FindType(assembly, @namespace, name);
			if (type == null)
			{
				return false;
			}
			return IsTypeValid(type, s_emptyArguments);
		}

		public ScriptStructure CreateStructure(string assembly, string name)
		{
			TypeDefinition type = FindType(assembly, name);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {name}[{assembly}]");
			}
			return new MonoStructure(type);
		}

		public ScriptStructure CreateStructure(string assembly, string @namespace, string name)
		{
			TypeDefinition type = FindType(assembly, @namespace, name);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {@namespace}.{name}[{assembly}]");
			}
			return new MonoStructure(type);
		}

		public ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string name)
		{
			TypeDefinition type = FindType(assembly, name);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {name}[{assembly}]");
			}
			return exportManager.CreateExportType(type);
		}

		public ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string @namespace, string name)
		{
			TypeDefinition type = FindType(assembly, @namespace, name);
			if (type == null)
			{
				throw new ArgumentException($"Can't find type {@namespace}.{name}[{assembly}]");
			}
			return exportManager.CreateExportType(type);
		}

		public ScriptInfo GetScriptInfo(string assembly, string name)
		{
			TypeDefinition type = FindType(assembly, name);
			if (type == null)
			{
				return default;
			}
			return new ScriptInfo(type.Scope.Name, type.Namespace, type.Name);
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			string assemblyName = AssemblyManager.ToAssemblyName(name.Name);
			AssemblyDefinition definition = RetrieveAssembly(assemblyName);
			if(definition == null)
			{
				const string MSCorLibName = "mscorlib";
				if (name.Name == MSCorLibName)
				{
					DefaultAssemblyResolver defaultResolver = new DefaultAssemblyResolver();
					definition = defaultResolver.Resolve(name);
					if (definition != null)
					{
						m_assemblies.Add(MSCorLibName, definition);
					}
				}
			}
			return definition;
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

		private void Dispose(bool disposing)
		{
			foreach (AssemblyDefinition assembly in m_assemblies.Values)
			{
				assembly.Dispose();
			}
		}

		private AssemblyDefinition RetrieveAssembly(string name)
		{
			if (m_assemblies.TryGetValue(name, out AssemblyDefinition assembly))
			{
				return assembly;
			}

			m_requestAssemblyCallback.Invoke(name);
			if (m_assemblies.TryGetValue(name, out assembly))
			{
				return assembly;
			}
			return null;
		}

		private TypeDefinition FindType(string assembly, string name)
		{
			AssemblyDefinition definition = RetrieveAssembly(assembly);
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
			AssemblyDefinition definition = RetrieveAssembly(assembly);
			if (definition == null)
			{
				return null;
			}

			foreach (ModuleDefinition module in definition.Modules)
			{
				foreach (TypeDefinition type in module.Types)
				{
					if (type.Name == name && type.Namespace == @namespace)
					{
						return type;
					}
				}
			}
			return null;
		}

		private bool IsTypeValid(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			if(type.IsGenericParameter)
			{
				GenericParameter parameter = (GenericParameter)type;
				return IsTypeValid(arguments[parameter], arguments);
			}
			if(type.IsArray)
			{
				return IsTypeValid(type.GetElementType(), arguments);
			}
			if(MonoType.IsList(type))
			{
				return IsListValid(type, arguments);
			}

			if(type.IsGenericParameter)
			{
				return true;
			}
			if (MonoType.IsPrime(type))
			{
				return true;
			}
			if (type.Module == null)
			{
				return false;
			}

			if (m_validTypes.TryGetValue(type.FullName, out bool isValid))
			{
				return isValid;
			}

			// set value at the beginning to prevent loop referencing
			m_validTypes[type.FullName] = true;

			if (type.IsGenericInstance)
			{
				// this is the case only for base classes. Field types aren't checked here
				GenericInstanceType instance = (GenericInstanceType)type;
				Dictionary<GenericParameter, TypeReference> templateArguments = new Dictionary<GenericParameter, TypeReference>();
				TypeReference template = instance.ElementType.ResolveOrDefault();
				for (int i = 0; i < instance.GenericArguments.Count; i++)
				{
					TypeReference argument = instance.GenericArguments[i];
					// don't check the validity of argument since it may not be used as a serialize field
					/*if (!IsTypeValid(argument, arguments))
					{
						m_validTypes[type.FullName] = false;
						return false;
					}*/
					templateArguments.Add(template.GenericParameters[i], argument);
				}

				return IsTypeValid(template, templateArguments);
			}

			TypeDefinition definition = type.Resolve();
			if(definition == null)
			{
				m_validTypes[type.FullName] = false;
				return false;
			}
			if(definition.IsInterface)
			{
				return true;
			}
			if (!IsTypeValid(definition.BaseType, arguments))
			{
				m_validTypes[type.FullName] = false;
				return false;
			}

			foreach (FieldDefinition field in definition.Fields)
			{
				if(!MonoField.IsSerializableModifier(field))
				{
					continue;
				}
				if (field.FieldType.IsGenericInstance)
				{
					// generic instances aren't serializable. Exception - list
					if (MonoType.IsList(type))
					{
						if (!IsListValid(type, arguments))
						{
							m_validTypes[type.FullName] = false;
							return false;
						}
					}
					continue;
				}

				if (!IsTypeValid(field.FieldType, arguments))
				{
					m_validTypes[type.FullName] = false;
					return false;
				}
			}
			return true;
		}

		private bool IsListValid(TypeReference type, IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
		{
			GenericInstanceType list = (GenericInstanceType)type;
			TypeReference element = list.GenericArguments[0];
			return IsTypeValid(element, arguments);
		}
		
		public ScriptingBackEnd ScriptingBackEnd
		{
			get => ScriptingBackEnd.Mono;
			set => throw new NotSupportedException();
		}

		private static readonly IReadOnlyDictionary<GenericParameter, TypeReference> s_emptyArguments = new Dictionary<GenericParameter, TypeReference>();

		public const string AssemblyExtension = ".dll";

		private readonly Dictionary<string, AssemblyDefinition> m_assemblies = new Dictionary<string, AssemblyDefinition>();
		private readonly Dictionary<string, bool> m_validTypes = new Dictionary<string, bool>();

		private event Action<string> m_requestAssemblyCallback;
	}
}
