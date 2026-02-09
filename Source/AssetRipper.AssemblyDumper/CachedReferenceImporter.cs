using AssetRipper.AssemblyDumper.Types;

namespace AssetRipper.AssemblyDumper;

public sealed class CachedReferenceImporter
{
	private readonly Dictionary<Type, ITypeDefOrRef> cachedTypeReferences = new();
	private readonly Dictionary<Type, TypeSignature> cachedTypeSignatureReferences = new();
	private readonly Dictionary<Type, TypeDefinition> cachedTypeDefinitions = new();
	private readonly HashSet<ModuleDefinition> referenceModules = new();

	public ReferenceImporter UnderlyingImporter { get; }
	public ModuleDefinition TargetModule => UnderlyingImporter.TargetModule;

	public CorLibTypeSignature Void => TargetModule.CorLibTypeFactory.Void;
	public CorLibTypeSignature Char => TargetModule.CorLibTypeFactory.Char;
	public CorLibTypeSignature Boolean => TargetModule.CorLibTypeFactory.Boolean;
	public CorLibTypeSignature Int8 => TargetModule.CorLibTypeFactory.SByte;
	public CorLibTypeSignature UInt8 => TargetModule.CorLibTypeFactory.Byte;
	public CorLibTypeSignature Int16 => TargetModule.CorLibTypeFactory.Int16;
	public CorLibTypeSignature UInt16 => TargetModule.CorLibTypeFactory.UInt16;
	public CorLibTypeSignature Int32 => TargetModule.CorLibTypeFactory.Int32;
	public CorLibTypeSignature UInt32 => TargetModule.CorLibTypeFactory.UInt32;
	public CorLibTypeSignature Int64 => TargetModule.CorLibTypeFactory.Int64;
	public CorLibTypeSignature UInt64 => TargetModule.CorLibTypeFactory.UInt64;
	public CorLibTypeSignature Single => TargetModule.CorLibTypeFactory.Single;
	public CorLibTypeSignature Double => TargetModule.CorLibTypeFactory.Double;
	public CorLibTypeSignature String => TargetModule.CorLibTypeFactory.String;
	public CorLibTypeSignature IntPtr => TargetModule.CorLibTypeFactory.IntPtr;
	public CorLibTypeSignature UIntPtr => TargetModule.CorLibTypeFactory.UIntPtr;
	public CorLibTypeSignature TypedReference => TargetModule.CorLibTypeFactory.TypedReference;
	public CorLibTypeSignature Object => TargetModule.CorLibTypeFactory.Object;

	public CachedReferenceImporter(ModuleDefinition module)
	{
		UnderlyingImporter = new ReferenceImporter(module);
	}

	public void AddReferenceModule(ModuleDefinition referenceModule) => referenceModules.Add(referenceModule);

	public TypeDefinition? LookupType<T>() => LookupType(typeof(T));
	public TypeDefinition? LookupType(Type type)
	{
		if (!cachedTypeDefinitions.TryGetValue(type, out TypeDefinition? typeDefinition)
			&& TryGetTypeDefinitionMatch(referenceModules, type.FullName!, out typeDefinition))
		{
			cachedTypeDefinitions.Add(type, typeDefinition);
		}
		return typeDefinition;
	}
	/// <summary>
	/// Does not use caching
	/// </summary>
	/// <param name="fullName"></param>
	/// <returns></returns>
	public TypeDefinition? LookupType(string fullName)
	{
		TryLookupType(fullName, out TypeDefinition? typeDefinition);
		return typeDefinition;
	}

	public bool TryLookupType<T>([NotNullWhen(true)] out TypeDefinition? typeDefinition)
	{
		typeDefinition = LookupType<T>();
		return typeDefinition != null;
	}
	public bool TryLookupType(Type type, [NotNullWhen(true)] out TypeDefinition? typeDefinition)
	{
		typeDefinition = LookupType(type);
		return typeDefinition != null;
	}
	/// <summary>
	/// Does not use caching
	/// </summary>
	/// <param name="fullName"></param>
	/// <param name="typeDefinition"></param>
	/// <returns></returns>
	public bool TryLookupType(string fullName, [NotNullWhen(true)] out TypeDefinition? typeDefinition)
	{
		return TryGetTypeDefinitionMatch(referenceModules, fullName, out typeDefinition);
	}

	public MethodDefinition LookupMethod<T>(Func<MethodDefinition, bool> filter) => LookupMethod(typeof(T), filter);
	public MethodDefinition LookupMethod(Type type, Func<MethodDefinition, bool> filter)
	{
		TypeDefinition typeDefinition = LookupType(type) ?? throw new ArgumentException($"Module for {type.FullName} not referenced", nameof(type));
		return typeDefinition.Methods.Single(filter);
	}

	public FieldDefinition LookupField<T>(string name) => LookupField(typeof(T), name);
	public FieldDefinition LookupField(Type type, string name)
	{
		TypeDefinition typeDefinition = LookupType(type) ?? throw new ArgumentException($"Module for {type.FullName} not referenced", nameof(type));
		return typeDefinition.GetFieldByName(name);
	}

	public IMethodDefOrRef ImportMethod<T>(Func<MethodDefinition, bool> filter) => UnderlyingImporter.ImportMethod(LookupMethod<T>(filter));
	public IMethodDefOrRef ImportMethod(Type type, Func<MethodDefinition, bool> filter) => UnderlyingImporter.ImportMethod(LookupMethod(type, filter));

	public IFieldDescriptor ImportField<T>(string name) => UnderlyingImporter.ImportField(LookupField<T>(name));
	public IFieldDescriptor ImportField(Type type, string name) => UnderlyingImporter.ImportField(LookupField(type, name));

	public ITypeDefOrRef ImportType<T>() => ImportType(typeof(T));
	public ITypeDefOrRef ImportType(Type type)
	{
		if (!cachedTypeReferences.TryGetValue(type, out ITypeDefOrRef? result))
		{
			result = TryLookupType(type, out TypeDefinition? typeDefinition)
				? UnderlyingImporter.ImportType(typeDefinition)
				: UnderlyingImporter.ImportType(type);
			cachedTypeReferences.Add(type, result);
		}
		return result;
	}

	public TypeSignature ImportTypeSignature<T>() => ImportTypeSignature(typeof(T));
	public TypeSignature ImportTypeSignature(Type type)
	{
		if (!cachedTypeSignatureReferences.TryGetValue(type, out TypeSignature? result))
		{
			result = TryLookupType(type, out TypeDefinition? typeDefinition)
				? UnderlyingImporter.ImportTypeSignature(typeDefinition.ToTypeSignature())
				: UnderlyingImporter.ImportTypeSignature(type);
			cachedTypeSignatureReferences.Add(type, result);
		}
		return result;
	}

	private static bool TryGetTypeDefinitionMatch(IEnumerable<ModuleDefinition> modules, string fullName, [NotNullWhen(true)] out TypeDefinition? type)
	{
		foreach (ModuleDefinition module in modules)
		{
			if (TryGetTypeDefinitionMatch(module, fullName, out type))
			{
				return true;
			}
		}
		type = null;
		return false;
	}

	private static bool TryGetTypeDefinitionMatch(ModuleDefinition module, string fullName, [NotNullWhen(true)] out TypeDefinition? type)
	{
		for (int i = 0; i < module.TopLevelTypes.Count; i++)
		{
			if (module.TopLevelTypes[i].FullName == fullName)
			{
				type = module.TopLevelTypes[i];
				return true;
			}
		}
		for (int i = 0; i < module.TopLevelTypes.Count; i++)
		{
			if (TryGetTypeDefinitionMatch(module.TopLevelTypes[i], fullName, out type))
			{
				return true;
			}
		}
		type = null;
		return false;
	}

	private static bool TryGetTypeDefinitionMatch(TypeDefinition parent, string fullName, [NotNullWhen(true)] out TypeDefinition? type)
	{
		for (int i = 0; i < parent.NestedTypes.Count; i++)
		{
			if (parent.NestedTypes[i].FullName == fullName)
			{
				type = parent.NestedTypes[i];
				return true;
			}
		}
		for (int i = 0; i < parent.NestedTypes.Count; i++)
		{
			if (TryGetTypeDefinitionMatch(parent.NestedTypes[i], fullName, out type))
			{
				return true;
			}
		}
		type = null;
		return false;
	}
}