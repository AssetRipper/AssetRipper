using AssetRipper.Assets;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

/// <summary>
/// Reads the <see cref="ManagedReferenceTypes.Registry"/> containing the objects referenced by an asset's [SerializeReference] fields.
/// </summary>
/// <remarks>
/// The registry has two formats, and its type tree only describes the one that the writing Unity version used.
/// In the first, the objects are terminated by an entry with the terminus type and an object's identifier is its index.
/// In the second, the objects are preceded by a count and each object stores its own identifier.
/// Both are read into <see cref="ManagedReferenceTypes.Registry"/>, so that the layout does not depend on the format.
/// </remarks>
internal static class ManagedReferenceRegistryReader
{
	private const int TerminatedFormat = 1;
	private const int CountedFormat = 2;

	private const string TerminusClassName = "Terminus";
	private const string TerminusNamespace = "UnityEngine.DMAT";
	private const string TerminusAssemblyName = "FAKE_ASM";

	/// <summary>
	/// The identifier of a reference to an object that could not be deserialized.
	/// </summary>
	private const long UnknownReferenceId = -1;

	/// <summary>
	/// The identifier of a null reference.
	/// </summary>
	private const long NullReferenceId = -2;

	public static SerializableStructure Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, ITypeResolver resolver)
	{
		SerializableStructure registry = new(ManagedReferenceTypes.Registry, depth, version);
		int format = reader.ReadInt32();
		registry[ManagedReferenceTypes.VersionFieldName].AsInt32 = format;
		registry[ManagedReferenceTypes.ReferenceIdsFieldName].AsAssetArray = format switch
		{
			TerminatedFormat => ReadTerminatedObjects(ref reader, version, flags, depth + 1, resolver),
			CountedFormat => ReadCountedObjects(ref reader, version, flags, depth + 1, resolver),
			_ => throw new NotSupportedException($"Managed reference registry format {format} is not supported."),
		};

		//The array of referenced objects is aligned, which is not necessarily true of the objects themselves.
		reader.Align();

		return registry;
	}

	private static IUnityAssetBase[] ReadTerminatedObjects(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, ITypeResolver resolver)
	{
		List<IUnityAssetBase> referencedObjects = [];
		while (true)
		{
			if (reader.Position >= reader.Length)
			{
				throw new EndOfStreamException($"The managed reference registry ended after {referencedObjects.Count} objects without a terminus.");
			}

			SerializableStructure managedType = ReadManagedType(ref reader, version, flags, depth + 1, resolver);
			if (IsTerminus(managedType))
			{
				return referencedObjects.ToArray();
			}

			referencedObjects.Add(ReadReferencedObject(ref reader, version, flags, depth, resolver, managedType, referencedObjects.Count));
		}
	}

	private static IUnityAssetBase[] ReadCountedObjects(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, ITypeResolver resolver)
	{
		int count = reader.ReadInt32();
		long remainingBytes = reader.Length - reader.Position;
		if (remainingBytes < count)
		{
			throw new EndOfStreamException($"The stream only has {remainingBytes} bytes remaining, so {count} referenced objects cannot be read.");
		}

		IUnityAssetBase[] referencedObjects = new IUnityAssetBase[count];
		for (int i = 0; i < count; i++)
		{
			long referenceId = reader.ReadInt64();
			SerializableStructure managedType = ReadManagedType(ref reader, version, flags, depth + 1, resolver);
			referencedObjects[i] = ReadReferencedObject(ref reader, version, flags, depth, resolver, managedType, referenceId);
		}
		return referencedObjects;
	}

	private static SerializableStructure ReadManagedType(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, ITypeResolver resolver)
	{
		SerializableStructure managedType = new(ManagedReferenceTypes.ReferencedManagedType, depth);
		managedType.Read(ref reader, version, flags, resolver);
		return managedType;
	}

	private static SerializableStructure ReadReferencedObject(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, ITypeResolver resolver, SerializableStructure managedType, long referenceId)
	{
		SerializableStructure referencedObject = new(ManagedReferenceTypes.ReferencedObject, depth, version);
		referencedObject[ManagedReferenceTypes.ReferenceIdFieldName].AsInt64 = referenceId;
		referencedObject[ManagedReferenceTypes.TypeFieldName].AsAsset = managedType;
		referencedObject[ManagedReferenceTypes.DataFieldName].AsAsset = ReadReferencedObjectData(ref reader, version, flags, depth + 1, resolver, managedType, referenceId);
		return referencedObject;
	}

	private static IUnityAssetBase ReadReferencedObjectData(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags, int depth, ITypeResolver resolver, SerializableStructure managedType, long referenceId)
	{
		string className = managedType[ManagedReferenceTypes.ClassFieldName].AsString;
		string assemblyName = managedType[ManagedReferenceTypes.AssemblyFieldName].AsString;
		if (referenceId is UnknownReferenceId or NullReferenceId || className.Length == 0 || assemblyName.Length == 0)
		{
			//Null and unknown references have no content.
			return new SerializableStructure(ManagedReferenceTypes.ReferencedObjectData, depth, version);
		}

		string namespaceName = managedType[ManagedReferenceTypes.NamespaceFieldName].AsString;
		ScriptIdentifier scriptID = new(SpecialFileNames.RemoveAssemblyFileExtension(assemblyName), namespaceName, className);
		if (!resolver.TryGetSerializableType(scriptID, version, out SerializableType? serializableType, out string? failureReason))
		{
			throw new InvalidDataException($"Could not resolve the referenced type {scriptID.UniqueName}. Reason: {failureReason}");
		}

		SerializableStructure data = new(serializableType, depth);
		data.Read(ref reader, version, flags, resolver);
		return data;
	}

	private static bool IsTerminus(SerializableStructure managedType)
	{
		return managedType[ManagedReferenceTypes.ClassFieldName].AsString == TerminusClassName
			&& managedType[ManagedReferenceTypes.NamespaceFieldName].AsString == TerminusNamespace
			&& managedType[ManagedReferenceTypes.AssemblyFieldName].AsString == TerminusAssemblyName;
	}
}
