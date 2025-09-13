using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.Builder;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Managers;

public partial class BaseManager : IAssemblyManager
{
	public bool IsSet => ScriptingBackend != ScriptingBackend.Unknown;
	public virtual ScriptingBackend ScriptingBackend => ScriptingBackend.Unknown;

	protected readonly Dictionary<string, AssemblyDefinition?> m_assemblies = new();
	protected readonly Dictionary<AssemblyDefinition, Stream> m_assemblyStreams = new(SignatureComparer.Default);
	protected readonly Dictionary<string, bool> m_validTypes = new();
	private readonly Dictionary<FieldSerializer, Dictionary<ITypeDefOrRef, SerializableType>> monoTypeCache = new();

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

	protected static string GetUniqueName(ITypeDefOrRef type)
	{
		string assembly = SpecialFileNames.RemoveAssemblyFileExtension(type.Scope?.Name ?? "");
		return ScriptIdentifier.ToUniqueName(assembly, type.FullName);
	}

	public virtual void Load(string filePath, FileSystem fileSystem)
	{
		Stream stream = fileSystem.File.OpenRead(filePath);
		AssemblyDefinition assembly;
		try
		{
			assembly = AssemblyDefinition.FromStream(stream);
		}
		catch (BadImageFormatException badImageFormatException)
		{
			throw new BadImageFormatException($"Could not read {filePath}", badImageFormatException);
		}

		string fileName = fileSystem.Path.GetFileNameWithoutExtension(filePath);
		m_assemblies.Add(fileName, assembly);

		m_assemblyStreams.Add(assembly, stream);

		Add(assembly);
	}

	public void Add(AssemblyDefinition assembly)
	{
		assembly.InitializeResolvers(this);
		string assemblyName = ToAssemblyName(assembly);
		m_assemblies[assemblyName] = assembly;
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
			assembly.ManifestModule?.ToPEImage(new ManagedPEImageBuilder(), false).ToPEFile(new ManagedPEFileBuilder()).Write(memoryStream);
			m_assemblyStreams.Add(assembly, memoryStream);
			return memoryStream;
		}
	}

	public void ClearStreamCache()
	{
		m_assemblyStreams.Clear();
	}

	private static string ToAssemblyName(AssemblyDefinition assembly)
	{
		return SpecialFileNames.RemoveAssemblyFileExtension(assembly.Name?.ToString() ?? "");
	}

	public virtual void Read(Stream stream, string fileName)
	{
		AssemblyDefinition assembly = AssemblyDefinition.FromStream(stream);
		assembly.InitializeResolvers(this);
		fileName = Path.GetFileNameWithoutExtension(fileName);
		string assemblyName = ToAssemblyName(assembly);
		m_assemblies.Add(fileName, assembly);
		m_assemblies[assemblyName] = assembly;
		m_assemblyStreams.Add(assembly, stream);
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

	public bool TryGetSerializableType(
		ScriptIdentifier scriptID,
		UnityVersion version,
		[NotNullWhen(true)] out SerializableType? scriptType,
		[NotNullWhen(false)] out string? failureReason)
	{
		if (m_serializableTypes.TryGetValue(scriptID.UniqueName, out scriptType))
		{
			failureReason = null;
			return true;
		}
		TypeDefinition? type = FindType(scriptID);
		if (type is null)
		{
			scriptType = null;
			failureReason = $"Can't find type: {scriptID.UniqueName}";
			return false;
		}
		else
		{
			FieldSerializer fieldSerializer = new(version);
			if (!monoTypeCache.TryGetValue(fieldSerializer, out Dictionary<ITypeDefOrRef, SerializableType>? typeCache))
			{
				typeCache = new(SignatureComparer.Default);
				monoTypeCache[fieldSerializer] = typeCache;
			}

			if (typeCache.TryGetValue(type, out SerializableType? monoType)
				|| fieldSerializer.TryCreateSerializableType(type, typeCache, out monoType, out failureReason))
			{
				scriptType = monoType;
				failureReason = null;
				return true;
			}
			else
			{
				scriptType = null;
				return false;
			}
		}
	}

	internal void AddSerializableType(ITypeDefOrRef type, SerializableType scriptType)
	{
		string uniqueName = GetUniqueName(type);
		AddSerializableType(uniqueName, scriptType);
	}

	internal void InvokeRequestAssemblyCallback(string assemblyName) => m_requestAssemblyCallback.Invoke(assemblyName);

	internal void AddSerializableType(string uniqueName, SerializableType scriptType) => m_serializableTypes.Add(uniqueName, scriptType);

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

	public virtual IEnumerable<AssemblyDefinition> GetAssemblies()
	{
		return m_assemblies.Values.Where(x => x is not null).Distinct()!;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (disposing)
		{
			ClearStreamCache();
		}
	}

	~BaseManager() => Dispose(false);
}
