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
using System.Text;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

public sealed class SerializableStructure : UnityAssetBase, IDeepCloneable
{
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
		Read(ref reader, version, flags, null);
	}

	internal void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, ManagedReferenceResolver? managedReferenceResolver)
	{
		Version = version;
		for (int i = 0; i < Fields.Length; i++)
		{
			SerializableType.Field etalon = Type.Fields[i];
			if (IsAvailable(etalon))
			{
				int startPosition = reader.Position;
				try
				{
					Fields[i].Read(ref reader, version, flags, Depth, etalon, managedReferenceResolver);
				}
				catch
				{
					if (ShouldTraceManagedReferenceLayout(Type.Name))
					{
						Logger.Info(LogCategory.Import, $"ManagedRefTrace {Type.Name}.{etalon.Name} failed at {startPosition}");
						if (etalon.Name == "references")
						{
							Logger.Info(LogCategory.Import, $"ManagedRefTrace {Type.Name}.references bytes at {startPosition}: {DumpBytes(reader, startPosition)}");
						}
					}
					throw;
				}
				if (ShouldTraceManagedReferenceLayout(Type.Name))
				{
					Logger.Info(LogCategory.Import, $"ManagedRefTrace {Type.Name}.{etalon.Name}: {startPosition} -> {reader.Position}");
				}
			}
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
		return TryRead(ref reader, monoBehaviour, false, null, true);
	}

	public bool TryRead(ref EndianSpanReader reader, IMonoBehaviour monoBehaviour, bool allowTrailingBytes)
	{
		return TryRead(ref reader, monoBehaviour, allowTrailingBytes, null, true);
	}

	internal bool TryRead(ref EndianSpanReader reader, IMonoBehaviour monoBehaviour, bool allowTrailingBytes, ManagedReferenceResolver? managedReferenceResolver, bool logFailure = true)
	{
		try
		{
			Read(ref reader, monoBehaviour.Collection.Version, monoBehaviour.Collection.Flags, managedReferenceResolver);
		}
		catch (Exception ex)
		{
			if (logFailure)
			{
				LogMonoBehaviorReadException(this, ex);
			}
			return false;
		}
		if (!allowTrailingBytes && reader.Position != reader.Length)
		{
			if (logFailure)
			{
				LogMonoBehaviourMismatch(this, reader.Position, reader.Length);
			}
			return false;
		}
		return true;
	}

	private static void LogMonoBehaviourMismatch(SerializableStructure structure, int actual, int expected)
	{
		Logger.Error(LogCategory.Import, $"Unable to read MonoBehaviour Structure, because script {structure} layout mismatched binary content (read {actual} bytes, expected {expected} bytes).");
	}

	private static void LogMonoBehaviorReadException(SerializableStructure structure, Exception ex)
	{
		Logger.Error(LogCategory.Import, $"Unable to read MonoBehaviour Structure, because script {structure} layout mismatched binary content ({ex.GetType().Name}).");
	}

	private static bool ShouldTraceManagedReferenceLayout(string typeName)
	{
		if (Environment.GetEnvironmentVariable("AR_DEBUG_MANAGED_REFERENCE") is not "1")
		{
			return false;
		}

		return typeName is "RequestableOwnershipGuard" or "TransferrableItemSlotTransformOverride" or "SlotTransformOverride" or "NetworkProjectConfigAsset";
	}

	private static string DumpBytes(EndianSpanReader reader, int offset)
	{
		int safeOffset = Math.Max(0, Math.Min(offset, reader.Length));
		int length = Math.Min(96, Math.Max(0, reader.Length - safeOffset));
		if (length == 0)
		{
			return "<none>";
		}

		EndianSpanReader copy = reader;
		copy.Position = safeOffset;
		StringBuilder builder = new(length * 3);
		for (int i = 0; i < length; i++)
		{
			if (i > 0)
			{
				builder.Append(' ');
			}
			builder.Append(copy.ReadByte().ToString("X2"));
		}
		return builder.ToString();
	}

	public bool CanUseLossyManagedReferenceFallback()
	{
		if (Fields.Length == 0)
		{
			return false;
		}

		SerializableType.Field lastField = Type.Fields[^1];
		if (lastField.Name != "references" || lastField.Type.Name != "ManagedReferencesRegistry")
		{
			return false;
		}

		for (int i = 0; i < Fields.Length - 1; i++)
		{
			if (Fields[i].PValue != 0 || Fields[i].CValue is not null)
			{
				return true;
			}
		}

		return false;
	}

	public void ApplyLossyManagedReferenceFallbackFixups()
	{
		if (!CanUseLossyManagedReferenceFallback())
		{
			return;
		}

		ManagedReferencesRegistryAsset registry = GetOrCreateFallbackRegistry();
		bool foundManagedReference = false;

		for (int i = 0; i < Fields.Length - 1; i++)
		{
			SerializableType.Field field = Type.Fields[i];
			if (field.ArrayDepth == 0 && field.Type.Name == "managedReference" && Fields[i].CValue is SerializableStructure managedReference)
			{
				foundManagedReference = true;
				ref SerializableValue rid = ref managedReference["rid"];
				if (rid.AsInt64 == 0)
				{
					rid.AsInt64 = -2;
				}
				registry.EnsureNullReference(rid.AsInt64);
			}
		}

		if (!foundManagedReference && registry.References.Count == 0)
		{
			Fields[^1] = default;
		}
	}

	private ManagedReferencesRegistryAsset GetOrCreateFallbackRegistry()
	{
		ref SerializableValue registryField = ref Fields[^1];
		if (registryField.CValue is ManagedReferencesRegistryAsset existingRegistry)
		{
			if (existingRegistry.Version == 0)
			{
				existingRegistry.InitializeFallback(2);
			}
			return existingRegistry;
		}

		ManagedReferencesRegistryAsset registry = new(null);
		registry.InitializeFallback(2);
		registryField.AsAsset = registry;
		return registry;
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
	}

	public void InitializeFields(UnityVersion version)
	{
		Version = version;
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
}
