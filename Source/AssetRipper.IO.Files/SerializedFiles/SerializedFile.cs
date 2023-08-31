using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Converters;
using AssetRipper.IO.Files.SerializedFiles.IO;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile : FileBase
	{
		private FileIdentifier[]? m_dependencies;
		private ObjectInfo[]? m_objects;
		private SerializedType[]? m_types;
		private SerializedTypeReference[]? m_refTypes;

		public FormatVersion Generation { get; private set; }
		public UnityVersion Version { get; private set; }
		public BuildTarget Platform { get; private set; }
		public TransferInstructionFlags Flags { get; private set; }
		public EndianType EndianType { get; private set; }
		public ReadOnlySpan<FileIdentifier> Dependencies => m_dependencies;
		public ReadOnlySpan<ObjectInfo> Objects => m_objects;
		public ReadOnlySpan<SerializedType> Types => m_types;
		public ReadOnlySpan<SerializedTypeReference> RefTypes => m_refTypes;
		public bool HasTypeTree { get; private set; }

		private static TransferInstructionFlags GetFlags(SerializedFileHeader header, SerializedFileMetadata metadata, string name, string filePath)
		{
			TransferInstructionFlags flags;
			if (SerializedFileMetadata.HasPlatform(header.Version) && metadata.TargetPlatform == BuildTarget.NoTarget)
			{
				if (filePath.EndsWith(".unity", StringComparison.Ordinal))
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

			if (FilenameUtils.IsEngineResource(name) || (header.Version < FormatVersion.Unknown_10 && FilenameUtils.IsBuiltinExtra(name)))
			{
				flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
			}
			if (header.Endianess || metadata.SwapEndianess)
			{
				flags |= TransferInstructionFlags.SwapEndianess;
			}
			return flags;
		}

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

			foreach (ObjectInfo objectInfo in metadata.Object)
			{
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
			Flags = GetFlags(header, metadata, Name, FilePath);
			EndianType = GetEndianType(header, metadata);
			m_dependencies = metadata.Externals;
			m_objects = metadata.Object;
			m_types = metadata.Types;
			m_refTypes = metadata.RefTypes;
			HasTypeTree = metadata.EnableTypeTree;
		}

		public override void Write(Stream stream)
		{
			long initialPosition = stream.Position;
			using SerializedWriter writer = new(stream, EndianType, Generation, Version);
			SerializedFileHeader header = new()
			{
				Version = Generation,
				Endianess = EndianType == EndianType.BigEndian,
			};
			header.Write(writer);

			SerializedFileMetadata metadata = new()
			{
				UnityVersion = Version,
				TargetPlatform = Platform,
				Externals = m_dependencies ?? Array.Empty<FileIdentifier>(),
				Object = GetNewObjectInfoArray(m_objects),
				Types = m_types ?? Array.Empty<SerializedType>(),
				RefTypes = m_refTypes ?? Array.Empty<SerializedTypeReference>(),
				EnableTypeTree = HasTypeTree,
			};
			long metadataPosition;
			long metadataSize;
			long objectDataPosition;
			if (SerializedFileMetadata.IsMetadataAtTheEnd(Generation))
			{
				metadataPosition = stream.Position;
				metadata.Write(writer);
				metadataSize = stream.Position - metadataPosition;
				AlignStream(writer);//Object data must always be aligned.
				objectDataPosition = stream.Position;
				WriteObjectData(writer, metadata.Object);
			}
			else
			{
				AlignStream(writer);//Object data must always be aligned.
				objectDataPosition = stream.Position;
				WriteObjectData(writer, metadata.Object);
				metadataPosition = stream.Position;
				metadata.Write(writer);
				metadataSize = stream.Position - metadataPosition;
			}
			
			long finalPosition = stream.Position;

			stream.Position = initialPosition;
			header.FileSize = finalPosition - initialPosition;
			header.MetadataSize = metadataSize;
			header.DataOffset = objectDataPosition - initialPosition;
			header.Write(writer);

			stream.Position = finalPosition;

			static void WriteObjectData(SerializedWriter writer, ObjectInfo[] objects)
			{
				foreach (ObjectInfo objectInfo in objects)
				{
					writer.Write(objectInfo.ObjectData);
					AlignStream(writer);
				}
			}

			static ObjectInfo[] GetNewObjectInfoArray(ObjectInfo[]? objects)
			{
				if (objects is null)
				{
					return Array.Empty<ObjectInfo>();
				}

				ObjectInfo[] newObjects = new ObjectInfo[objects.Length];

				//This doesn't work correctly because ObjectInfo is not a struct, but that can be fixed later.
				Array.Copy(objects, newObjects, objects.Length);

				long byteStart = 0;
				for (int i = 0; i < newObjects.Length; i++)
				{
					ObjectInfo objectInfo = newObjects[i];
					objectInfo.ByteStart = byteStart;
					objectInfo.ByteSize = objectInfo.ObjectData.Length;
					newObjects[i] = objectInfo;

					byteStart += objectInfo.ByteSize;
					byteStart += 3 - (byteStart % 4);//Object data must always be aligned.
				}

				return newObjects;
			}

			static void AlignStream(SerializedWriter writer)
			{
				switch (writer.BaseStream.Position % 4)
				{
					case 1:
						writer.Write((byte)0);
						writer.Write((byte)0);
						writer.Write((byte)0);
						break;
					case 2:
						writer.Write((byte)0);
						writer.Write((byte)0);
						break;
					case 3:
						writer.Write((byte)0);
						break;
				}
			}
		}

		public static SerializedFile FromFile(string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			SmartStream stream = SmartStream.OpenRead(filePath);
			return SerializedFileScheme.Default.Read(stream, filePath, fileName);
		}
	}
}
