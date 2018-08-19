using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Exporters.Scripts;

namespace UtinyRipper.AssetExporters.Mono
{
	internal class MonoManager : IAssemblyManager, IAssemblyResolver
	{
		public MonoManager(Action<string> requestAssemblyCallback)
		{
			if(requestAssemblyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestAssemblyCallback));
			}
			m_requestAssemblyCallback = requestAssemblyCallback;
		}

		public static bool IsMonoAssembly(string fileName)
		{
			if (fileName.EndsWith(".dll", StringComparison.Ordinal))
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
			AddAssembly(assembly, filePath);
		}

		public void Read(Stream stream, string filePath)
		{
			ReaderParameters parameters = new ReaderParameters(ReadingMode.Immediate)
			{
				InMemory = true,
				ReadWrite = false,
				AssemblyResolver = this,
			};
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(stream, parameters);
			AddAssembly(assembly, filePath);
		}

		public void Unload(string fileName)
		{
			if(m_assemblies.TryGetValue(fileName, out AssemblyDefinition assembly))
			{
				assembly.Dispose();
				m_assemblies.Remove(fileName);
			}
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
			return IsTypeValid(type);
		}

		public bool IsValid(string assembly, string @namespace, string name)
		{
			TypeDefinition type = FindType(assembly, @namespace, name);
			if (type == null)
			{
				return false;
			}
			return IsTypeValid(type);
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

		public void Dispose()
		{
			foreach(AssemblyDefinition assembly in m_assemblies.Values)
			{
				assembly.Dispose();
			}
			m_assemblies.Clear();
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			if(m_assemblies.TryGetValue(name.Name, out AssemblyDefinition assembly))
			{
				return assembly;
			}

			m_requestAssemblyCallback.Invoke(name.Name);
			if (m_assemblies.TryGetValue(name.Name, out assembly))
			{
				return assembly;
			}
			return null;
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			return Resolve(name);
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

		private bool IsTypeValid(TypeReference reference)
		{
			if(m_validTypes.TryGetValue(reference.FullName, out bool isValid))
			{
				return isValid;
			}

			// set value at the beginning to prevent loop referencing
			m_validTypes[reference.FullName] = true;
			if (MonoType.IsPrime(reference))
			{
				return true;
			}
			if (reference.Module == null)
			{
				m_validTypes[reference.FullName] = false;
				return false;
			}

			if(reference.IsGenericInstance)
			{
				GenericInstanceType generic = (GenericInstanceType)reference;
				foreach(TypeReference genericArg in generic.GenericArguments)
				{
					if (!IsTypeValid(genericArg))
					{
						m_validTypes[reference.FullName] = false;
						return false;
					}
				}
			}

			TypeDefinition definition = reference.Resolve();
			isValid = IsTypeValid(definition.BaseType);
			if(!isValid)
			{
				m_validTypes[reference.FullName] = false;
				return false;
			}

			foreach(FieldDefinition field in definition.Fields)
			{
				if (!MonoField.IsSerializableField(field))
				{
					continue;
				}

				isValid = IsTypeValid(field.FieldType);
				if(!isValid)
				{
					m_validTypes[reference.FullName] = false;
					return false;
				}
			}
			return true;
		}
		
		private void AddAssembly(AssemblyDefinition assembly, string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			m_assemblies.Add(fileName, assembly);
			fileName = Path.GetFileNameWithoutExtension(fileName);
			m_assemblies.Add(fileName, assembly);
		}
		
		public ScriptingBackEnd ScriptingBackEnd
		{
			get => ScriptingBackEnd.Mono;
			set => throw new NotSupportedException();
		}

		private readonly Dictionary<string, AssemblyDefinition> m_assemblies = new Dictionary<string, AssemblyDefinition>();
		private readonly Dictionary<string, bool> m_validTypes = new Dictionary<string, bool>();

		private event Action<string> m_requestAssemblyCallback;
	}
}
