using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Converters;
using AssetRipper.IO.Files.SerializedFiles.IO;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.Smart;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.IO.Files.SerializedFiles;

/// <summary>
/// Serialized files contain binary serialized objects and optional run-time type information.
/// They have file name extensions like .asset, .assets, .sharedAssets but may also have no extension at all
/// </summary>
public sealed class SerializedFile : FileBase
{
	private FileIdentifier[]? m_dependencies;
	private ObjectInfo[]? m_objects;
	private SerializedType[]? m_types;
	private LocalSerializedObjectIdentifier[]? m_scriptTypes;
	private SerializedTypeReference[]? m_refTypes;

	public FormatVersion Generation { get; private set; }
	public UnityVersion Version { get; private set; }
	public BuildTarget Platform { get; private set; }
	public TransferInstructionFlags Flags
	{
		get
		{
			TransferInstructionFlags flags;
			if (SerializedFileMetadata.HasPlatform(Generation) && Platform == BuildTarget.NoTarget)
			{
				if (FilePath.EndsWith(".unity", StringComparison.Ordinal))
				{
					flags = TransferInstructionFlags.SerializeEditorMinimalScene;
				}
				else
				{
					flags = TransferInstructionFlags.NoTransferInstructionFlags;
				}
			}
			else
			{
				flags = TransferInstructionFlags.SerializeGameRelease;
			}

			if (SpecialFileNames.IsEngineResource(Name) || (Generation < FormatVersion.Unknown_10 && SpecialFileNames.IsBuiltinExtra(Name)))
			{
				flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
			}
			if (EndianType is EndianType.BigEndian)
			{
				flags |= TransferInstructionFlags.SwapEndianess;
			}
			return flags;
		}
	}
	public EndianType EndianType { get; private set; }
	public ReadOnlySpan<FileIdentifier> Dependencies => m_dependencies;
	public ReadOnlySpan<ObjectInfo> Objects => m_objects;
	public ReadOnlySpan<SerializedType> Types => m_types;
	public ReadOnlySpan<LocalSerializedObjectIdentifier> ScriptTypes => m_scriptTypes;
	public ReadOnlySpan<SerializedTypeReference> RefTypes => m_refTypes;
	public bool HasTypeTree { get; private set; }

	private static EndianType GetEndianType(SerializedFileHeader header, SerializedFileMetadata metadata)
	{
		bool swapEndianess = SerializedFileHeader.HasEndianess(header.Version) ? header.Endianess : metadata.SwapEndianess;
		return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
	}

	public static bool IsSerializedFile(Stream stream)
	{
		using EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
		return SerializedFileHeader.IsSerializedFileHeader(reader, stream.Length);
	}

	public override string ToString()
	{
		return NameFixed;
	}

	public override void Read(SmartStream stream)
	{
		SerializedFileHeader header = new();
		using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
		{
			header.Read(reader);
		}
		if (SerializedFileMetadata.IsMetadataAtTheEnd(header.Version))
		{
			stream.Position = header.FileSize - header.MetadataSize;
		}
		SerializedFileMetadata metadata = new();
		metadata.Read(stream, header);

		SerializedFileMetadataConverter.CombineFormats(header.Version, metadata);

		for (int i = 0; i < metadata.Object.Length; i++)
		{
			ref ObjectInfo objectInfo = ref metadata.Object[i];
			stream.Position = header.DataOffset + objectInfo.ByteStart;
			byte[] objectData = new byte[objectInfo.ByteSize];
			stream.ReadExactly(objectData);
			objectInfo.ObjectData = objectData;
		}

		SetProperties(header, metadata);
	}

	private void SetProperties(SerializedFileHeader header, SerializedFileMetadata metadata)
	{
		Generation = header.Version;
		Version = metadata.UnityVersion;
		Platform = metadata.TargetPlatform;
		EndianType = GetEndianType(header, metadata);
		m_dependencies = metadata.Externals;
		m_objects = metadata.Object;
		m_types = metadata.Types;
		m_scriptTypes = metadata.ScriptTypes;
		m_refTypes = metadata.RefTypes;
		HasTypeTree = metadata.EnableTypeTree;
	}

	public override void Write(Stream stream)
	{
		long initialPosition = stream.Position;
		SerializedFileHeader header = new()
		{
			Version = Generation,
			Endianess = EndianType == EndianType.BigEndian,
		};
		header.Write(new EndianWriter(stream, EndianType.BigEndian));

		using SerializedWriter writer = new(stream, EndianType, Generation, Version);
		SerializedFileMetadata metadata = new()
		{
			UnityVersion = Version,
			TargetPlatform = Platform,
			Externals = m_dependencies ?? Array.Empty<FileIdentifier>(),
			Object = GetNewObjectInfoArray(m_objects),
			Types = m_types ?? Array.Empty<SerializedType>(),
			ScriptTypes = m_scriptTypes ?? Array.Empty<LocalSerializedObjectIdentifier>(),
			RefTypes = m_refTypes ?? Array.Empty<SerializedTypeReference>(),
			EnableTypeTree = HasTypeTree,
		};
		long metadataPosition;
		long metadataSize;
		long objectDataPosition;
		if (SerializedFileMetadata.IsMetadataAtTheEnd(Generation))
		{
			AlignStream(writer, 16); // objectDataPosition must be aligned to 16 bytes
			objectDataPosition = stream.Position;
			WriteObjectData(writer, metadata.Object);
			metadataPosition = stream.Position;
			metadata.Write(writer);
			metadataSize = stream.Position - metadataPosition;
		}
		else
		{
			metadataPosition = stream.Position;
			metadata.Write(writer);
			metadataSize = stream.Position - metadataPosition;
			AlignStream(writer, 16); // objectDataPosition must be aligned to 16 bytes
			objectDataPosition = stream.Position;
			WriteObjectData(writer, metadata.Object);
		}

		long finalPosition = stream.Position;

		stream.Position = initialPosition;
		header.FileSize = finalPosition - initialPosition;
		header.MetadataSize = metadataSize;
		header.DataOffset = objectDataPosition - initialPosition;
		header.Write(new EndianWriter(stream, EndianType.BigEndian));

		stream.Position = finalPosition;

		static void WriteObjectData(SerializedWriter writer, ObjectInfo[] objects)
		{
			foreach (ObjectInfo objectInfo in objects)
			{
				if (objectInfo.ObjectData is not null)
				{
					writer.Write(objectInfo.ObjectData);
				}
				AlignStream(writer, 8); // each object data must be aligned to 8 bytes
			}
		}

		static ObjectInfo[] GetNewObjectInfoArray(ObjectInfo[]? objects)
		{
			if (objects is null)
			{
				return Array.Empty<ObjectInfo>();
			}

			ObjectInfo[] newObjects = new ObjectInfo[objects.Length];
			Array.Copy(objects, newObjects, objects.Length);

			long byteStart = 0;
			for (int i = 0; i < newObjects.Length; i++)
			{
				ref ObjectInfo objectInfo = ref newObjects[i];
				objectInfo.ByteStart = byteStart;
				objectInfo.ByteSize = objectInfo.ObjectData?.Length ?? 0;

				byteStart += objectInfo.ByteSize;
				byteStart += 8 - (byteStart % 8); // each object data must be aligned to 8 bytes
			}

			return newObjects;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void AlignStream(SerializedWriter writer, [ConstantExpected] int alignment)
		{
			Debug.Assert(alignment > 0);
			Debug.Assert(BitOperations.IsPow2(alignment));
			long bytesSinceLastAlignment = writer.BaseStream.Position & (alignment - 1);
			if (bytesSinceLastAlignment != 0)
			{
				int padding = alignment - (int)bytesSinceLastAlignment;
				Span<byte> buffer = stackalloc byte[padding];
				buffer.Clear();
				writer.Write(buffer);
			}
		}
	}

	public static SerializedFile FromFile(string filePath, FileSystem fileSystem)
	{
		string fileName = fileSystem.Path.GetFileName(filePath);
		SmartStream stream = SmartStream.OpenRead(filePath, fileSystem);
		return SerializedFileScheme.Default.Read(stream, filePath, fileName);
	}

	public static SerializedFile FromBuilder(SerializedFileBuilder builder)
	{
		return new()
		{
			Generation = builder.Generation,
			Version = builder.Version,
			Platform = builder.Platform,
			EndianType = builder.EndianType,
			m_dependencies = builder.Dependencies.ToArray(),
			m_objects = builder.Objects.ToArray(),
			m_types = builder.Types.ToArray(),
			m_refTypes = builder.RefTypes.ToArray(),
			HasTypeTree = builder.HasTypeTree,
		};
	}
}
