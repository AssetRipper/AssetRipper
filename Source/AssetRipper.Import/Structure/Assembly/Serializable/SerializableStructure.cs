using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SerializationLogic;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

public sealed class SerializableStructure : UnityAssetBase, IDeepCloneable
{
	public readonly record struct ManagedReferenceTypeInfo(string ClassName, string Namespace, string Assembly);

	private UnityVersion Version { get; set; }
	public override int SerializedVersion => Type.Version;
	public override bool FlowMappedInYaml => Type.FlowMappedInYaml;

	internal SerializableStructure(SerializableType type, int depth)
	{
		Depth = depth;
		Type = type ?? throw new ArgumentNullException(nameof(type));
		Fields = new SerializableValue[type.Fields.Count];
	}

	public void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags)
	{
		Version = version;
		ManagedReferences.Clear();
		ManagedReferenceTypes.Clear();

		int managedReferencesFieldIndex = TryGetManagedReferencesFieldIndex();
		if (managedReferencesFieldIndex >= 0 && IsAvailable(Type.Fields[managedReferencesFieldIndex]))
		{
			EndianSpanReader managedFirstReader = reader;
			if (TryReadWithManagedReferencesFirst(ref managedFirstReader, version, flags, managedReferencesFieldIndex))
			{
				reader = managedFirstReader;
				return;
			}
			ClearFields();
		}

		for (int i = 0; i < Fields.Length; i++)
		{
			SerializableType.Field etalon = Type.Fields[i];
			if (IsAvailable(etalon))
			{
				Fields[i].Read(ref reader, version, flags, Depth, etalon);
			}
		}

		if (managedReferencesFieldIndex >= 0)
		{
			SerializableType.Field managedReferencesField = Type.Fields[managedReferencesFieldIndex];
			CacheManagedReferences(Fields[managedReferencesFieldIndex], managedReferencesField);
		}
	}

	public void Write(AssetWriter writer)
	{
		for (int i = 0; i < Fields.Length; i++)
		{
			SerializableType.Field etalon = Type.Fields[i];
			if (IsAvailable(etalon))
			{
				Fields[i].Write(writer, etalon);
			}
		}
	}
	public override void WriteEditor(AssetWriter writer) => Write(writer);
	public override void WriteRelease(AssetWriter writer) => Write(writer);

	public override void WalkEditor(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			bool hasEmittedFirstField = false;
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableType.Field etalon = Type.Fields[i];
				if (IsAvailable(etalon))
				{
					if (hasEmittedFirstField)
					{
						walker.DivideAsset(this);
					}
					else
					{
						hasEmittedFirstField = true;
					}
					if (walker.EnterField(this, etalon.Name))
					{
						Fields[i].WalkEditor(walker, etalon);
						walker.ExitField(this, etalon.Name);
					}
				}
			}
			walker.ExitAsset(this);
		}
	}
	//For now, only the editor version is implemented.
	public override void WalkRelease(AssetWalker walker) => WalkEditor(walker);
	public override void WalkStandard(AssetWalker walker) => WalkEditor(walker);

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		for (int i = 0; i < Fields.Length; i++)
		{
			SerializableType.Field etalon = Type.Fields[i];
			if (IsAvailable(etalon))
			{
				foreach ((string, PPtr) pair in Fields[i].FetchDependencies(etalon))
				{
					yield return pair;
				}
			}
		}

		foreach ((long rid, IUnityAssetBase referencedObject) in ManagedReferences)
		{
			foreach ((string path, PPtr pptr) in referencedObject.FetchDependencies())
			{
				yield return ($"references[{rid}].{path}", pptr);
			}
		}
	}

	public override string ToString() => Type.FullName;

	private bool IsAvailable(in SerializableType.Field field)
	{
		if (Depth <= GetMaxDepthLevel(Version))
		{
			return true;
		}
		if (field.ArrayDepth > 0)
		{
			return false;
		}
		if (field.Type.Type == PrimitiveType.Complex)
		{
			return MonoUtils.IsEngineStruct(field.Type.Namespace, field.Type.Name);
		}
		return true;
	}

	public bool TryRead(ref EndianSpanReader reader, IMonoBehaviour monoBehaviour)
	{
		try
		{
			Read(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags);
		}
		catch (Exception ex)
		{
			LogMonoBehaviorReadException(this, ex);
			return false;
		}
		if (reader.Position != reader.Length)
		{
			LogMonoBehaviourMismatch(this, reader.Position, reader.Length);
			return false;
		}
		return true;
	}

	private static void LogMonoBehaviourMismatch(SerializableStructure structure, int actual, int expected)
	{
		Logger.Verbose(LogCategory.Import, $"Unable to fully read MonoBehaviour Structure for script {structure} due layout mismatch (read {actual} bytes, expected {expected} bytes).");
	}

	private static void LogMonoBehaviorReadException(SerializableStructure structure, Exception ex)
	{
		Logger.Verbose(LogCategory.Import, $"Unable to fully read MonoBehaviour Structure for script {structure} due layout mismatch ({ex.GetType().Name}).");
	}

	public int Depth { get; }
	public SerializableType Type { get; }
	public SerializableValue[] Fields { get; }

	public ref SerializableValue this[string name]
	{
		get
		{
			if (TryGetIndex(name, out int index))
			{
				return ref Fields[index];
			}
			throw new KeyNotFoundException($"Field {name} wasn't found in {Type.Name}");
		}
	}

	public bool ContainsField(string name) => TryGetIndex(name, out _);

	public bool TryGetField(string name, out SerializableValue field)
	{
		if (TryGetIndex(name, out int index))
		{
			field = Fields[index];
			return true;
		}
		field = default;
		return false;
	}

	public SerializableValue? TryGetField(string name)
	{
		if (TryGetIndex(name, out int index))
		{
			return Fields[index];
		}
		return null;
	}

	public bool TryGetIndex(string name, out int index)
	{
		for (int i = 0; i < Fields.Length; i++)
		{
			if (Type.Fields[i].Name == name)
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}

	public override void CopyValues(IUnityAssetBase? source, PPtrConverter converter)
	{
		CopyValues((SerializableStructure?)source, converter);
	}

	public void CopyValues(SerializableStructure? source, PPtrConverter converter)
	{
		if (source is null)
		{
			Reset();
			return;
		}
		if (source.Depth != Depth)
		{
			throw new ArgumentException($"Depth {source.Depth} doesn't match with {Depth}", nameof(source));
		}
		Version = source.Version;
		if (source.Type == Type)
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				SerializableValue sourceField = source.Fields[i];
				if (sourceField.CValue is null)
				{
					Fields[i] = sourceField;
				}
				else
				{
					Fields[i].CopyValues(sourceField, Depth, Type.Fields[i], converter);
				}
			}
		}
		else
		{
			for (int i = 0; i < Fields.Length; i++)
			{
				string fieldName = Type.Fields[i].Name;
				int index = -1;
				for (int j = 0; j < source.Type.Fields.Count; j++)
				{
					if (fieldName == source.Type.Fields[j].Name)
					{
						index = j;
					}
				}
				SerializableValue sourceField = index < 0 ? default : source.Fields[index];
				Fields[i].CopyValues(sourceField, Depth, Type.Fields[i], converter);
			}
		}
		CopyManagedReferences(source, converter);
	}

	public SerializableStructure DeepClone(PPtrConverter converter)
	{
		SerializableStructure clone = new(Type, Depth);
		clone.CopyValues(this, converter);
		return clone;
	}

	IUnityAssetBase IDeepCloneable.DeepClone(PPtrConverter converter) => DeepClone(converter);

	public override void Reset()
	{
		foreach (SerializableValue field in Fields)
		{
			field.Reset();
		}
		ManagedReferences.Clear();
		ManagedReferenceTypes.Clear();
	}

	public void InitializeFields(UnityVersion version)
	{
		Version = version;
		ManagedReferences.Clear();
		ManagedReferenceTypes.Clear();
		for (int i = 0; i < Fields.Length; i++)
		{
			SerializableType.Field etalon = Type.Fields[i];
			if (IsAvailable(etalon))
			{
				Fields[i].Initialize(version, Depth, etalon);
			}
		}
	}

	/// <summary>
	/// Unity has a maximum serialization depth to prevent infinite recursion in cyclic references.
	/// In Unity versions prior to 2020.2.0a21, this limit is 7. From 2020.2.0a21 onwards, the limit was increased to 10.
	/// </summary>
	/// <remarks>
	/// <see href="https://forum.unity.com/threads/serialization-depth-limit-and-recursive-serialization.1263599/"/><br/>
	/// <see href="https://forum.unity.com/threads/getting-a-serialization-depth-limit-7-error-for-no-reason.529850/"/><br/>
	/// <see href="https://forum.unity.com/threads/4-5-serialization-depth.248321/"/>
	/// </remarks>
	private static int GetMaxDepthLevel(UnityVersion version) => version.GreaterThanOrEquals(2020, 2, 0, UnityVersionType.Alpha, 21) ? 10 : 7;

	public Dictionary<long, IUnityAssetBase> ManagedReferences { get; } = [];
	public Dictionary<long, ManagedReferenceTypeInfo> ManagedReferenceTypes { get; } = [];

	public bool IsManagedReferencesField(string name)
	{
		return TryGetIndex(name, out int index) && IsManagedReferencesField(Type.Fields[index]);
	}

	private int TryGetManagedReferencesFieldIndex()
	{
		for (int i = 0; i < Type.Fields.Count; i++)
		{
			if (IsManagedReferencesField(Type.Fields[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private static bool IsManagedReferencesField(in SerializableType.Field field)
	{
		if (field.ArrayDepth != 0 || field.Type.Type != PrimitiveType.Complex)
		{
			return false;
		}

		string normalizedFieldName = NormalizeManagedReferenceName(field.Name);
		string normalizedTypeName = NormalizeManagedReferenceName(field.Type.Name);
		return normalizedFieldName is "managedreferences" or "references" or "managedreferencesregistry"
			|| normalizedTypeName is "managedreferencesregistry" or "managedreference"
			|| normalizedFieldName.Contains("managedreference", StringComparison.Ordinal)
			|| normalizedTypeName.Contains("managedreference", StringComparison.Ordinal)
			|| LooksLikeManagedReferencesRegistry(field.Type);
	}

	private bool TryReadWithManagedReferencesFirst(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int managedReferencesFieldIndex)
	{
		try
		{
			SerializableType.Field managedReferencesField = Type.Fields[managedReferencesFieldIndex];
			Fields[managedReferencesFieldIndex].Read(ref reader, version, flags, Depth, managedReferencesField);
			CacheManagedReferences(Fields[managedReferencesFieldIndex], managedReferencesField);

			for (int i = 0; i < Fields.Length; i++)
			{
				if (i == managedReferencesFieldIndex)
				{
					continue;
				}

				SerializableType.Field etalon = Type.Fields[i];
				if (IsAvailable(etalon))
				{
					Fields[i].Read(ref reader, version, flags, Depth, etalon);
				}
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	private void ClearFields()
	{
		for (int i = 0; i < Fields.Length; i++)
		{
			Fields[i].Reset();
		}
		ManagedReferences.Clear();
		ManagedReferenceTypes.Clear();
	}

	private void CacheManagedReferences(SerializableValue managedReferencesFieldValue, in SerializableType.Field managedReferencesField)
	{
		if (managedReferencesField.ArrayDepth != 0
			|| managedReferencesField.Type.Type != PrimitiveType.Complex
			|| managedReferencesFieldValue.CValue is not SerializableStructure managedReferencesStructure)
		{
			return;
		}

		if (!TryGetManagedReferencesArray(managedReferencesStructure, out SerializableValue refIds))
		{
			return;
		}

		foreach (IUnityAssetBase referencedObjectAsset in refIds.AsAssetArray)
		{
			if (referencedObjectAsset is not SerializableStructure referencedObject)
			{
				continue;
			}

			if (!TryGetFieldByNormalizedName(referencedObject, "rid", out SerializableValue ridValue))
			{
				continue;
			}

			long rid = ridValue.AsInt64;
			if (rid == 0)
			{
				continue;
			}

			IUnityAssetBase data = EmptyAsset.Instance;
			if (TryGetFieldByNormalizedName(referencedObject, "data", out SerializableValue dataField) && dataField.CValue is IUnityAssetBase dataAsset)
			{
				data = dataAsset;
			}
			ManagedReferences[rid] = data;

			ManagedReferenceTypeInfo typeInfo = default;
			if (TryGetFieldByNormalizedName(referencedObject, "type", out SerializableValue typeField) && typeField.CValue is SerializableStructure typeStructure)
			{
				string className = TryGetFieldByNormalizedName(typeStructure, "class", out SerializableValue classField) ? classField.AsString : "";
				string @namespace = TryGetFieldByNormalizedName(typeStructure, "ns", out SerializableValue namespaceField) ? namespaceField.AsString : "";
				string assembly = TryGetFieldByNormalizedName(typeStructure, "asm", out SerializableValue assemblyField) ? assemblyField.AsString : "";
				typeInfo = new ManagedReferenceTypeInfo(className, @namespace, assembly);
			}
			ManagedReferenceTypes[rid] = typeInfo;
		}
	}

	private static bool LooksLikeManagedReferencesRegistry(SerializableType type)
	{
		foreach (SerializableType.Field subField in type.Fields)
		{
			string normalizedName = NormalizeManagedReferenceName(subField.Name);
			if (normalizedName is "refids" or "refid" or "references" or "managedreferences" or "data")
			{
				return true;
			}
		}
		return false;
	}

	private static bool TryGetManagedReferencesArray(SerializableStructure structure, out SerializableValue value)
	{
		for (int i = 0; i < structure.Type.Fields.Count; i++)
		{
			SerializableType.Field field = structure.Type.Fields[i];
			string normalized = NormalizeManagedReferenceName(field.Name);
			if (normalized is "refids" or "refid" or "data")
			{
				value = structure.Fields[i];
				return true;
			}
		}

		value = default;
		return false;
	}

	private static bool TryGetFieldByNormalizedName(SerializableStructure structure, string normalizedFieldName, out SerializableValue value)
	{
		for (int i = 0; i < structure.Type.Fields.Count; i++)
		{
			if (NormalizeManagedReferenceName(structure.Type.Fields[i].Name) == normalizedFieldName)
			{
				value = structure.Fields[i];
				return true;
			}
		}

		value = default;
		return false;
	}

	private static string NormalizeManagedReferenceName(string name)
	{
		if (name.StartsWith("m_", StringComparison.Ordinal))
		{
			name = name[2..];
		}
		return name.Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();
	}

	private void CopyManagedReferences(SerializableStructure source, PPtrConverter converter)
	{
		ManagedReferences.Clear();
		ManagedReferenceTypes.Clear();

		foreach ((long rid, IUnityAssetBase value) in source.ManagedReferences)
		{
			IUnityAssetBase clonedValue = value is IDeepCloneable cloneable
				? cloneable.DeepClone(converter)
				: value;
			ManagedReferences.Add(rid, clonedValue);
		}

		foreach ((long rid, ManagedReferenceTypeInfo value) in source.ManagedReferenceTypes)
		{
			ManagedReferenceTypes.Add(rid, value);
		}
	}
}
