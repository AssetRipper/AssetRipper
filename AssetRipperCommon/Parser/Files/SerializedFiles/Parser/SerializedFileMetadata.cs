using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files.SerializedFiles.IO;
using System.IO;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.Parser
{
	public sealed class SerializedFileMetadata
	{
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasEndian(FormatVersion generation) => generation < FormatVersion.Unknown_9;
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool IsMetadataAtTheEnd(FormatVersion generation) => generation < FormatVersion.Unknown_9;

		/// <summary>
		/// 3.0.0b and greater
		/// </summary>
		public static bool HasSignature(FormatVersion generation) => generation >= FormatVersion.Unknown_7;
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasPlatform(FormatVersion generation) => generation >= FormatVersion.Unknown_8;
		/// <summary>
		/// 5.0.0Unk2 and greater
		/// </summary>
		public static bool HasEnableTypeTree(FormatVersion generation) => generation >= FormatVersion.HasTypeTreeHashes;
		/// <summary>
		/// 3.0.0b to 4.x.x
		/// </summary>
		public static bool HasLongFileID(FormatVersion generation) => generation >= FormatVersion.Unknown_7 && generation < FormatVersion.Unknown_14;
		/// <summary>
		/// 5.0.0Unk0 and greater
		/// </summary>
		public static bool HasScriptTypes(FormatVersion generation) => generation >= FormatVersion.HasScriptTypeIndex;
		/// <summary>
		/// 1.2.0 and greater
		/// </summary>
		public static bool HasUserInformation(FormatVersion generation) => generation >= FormatVersion.Unknown_5;
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool HasRefTypes(FormatVersion generation) => generation >= FormatVersion.SupportsRefObject;

		public void Read(Stream stream, SerializedFileHeader header)
		{
			bool swapEndianess = header.Endianess;
			if (HasEndian(header.Version))
			{
				SwapEndianess = stream.ReadByte() != 0;
				swapEndianess = SwapEndianess;
			}
			EndianType endianess = swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			using SerializedReader reader = new SerializedReader(stream, endianess, header.Version);
			Read(reader);
		}

		public void Write(Stream stream, SerializedFileHeader header)
		{
			bool swapEndianess = header.Endianess;
			if (HasEndian(header.Version))
			{
				stream.WriteByte((byte)(SwapEndianess ? 1 : 0));
				swapEndianess = SwapEndianess;
			}
			EndianType endianess = swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			using SerializedWriter writer = new SerializedWriter(stream, endianess, header.Version);
			Write(writer);
		}

		private void Read(SerializedReader reader)
		{
			if (HasSignature(reader.Generation))
			{
				string signature = reader.ReadStringZeroTerm();
				UnityVersion = UnityVersion.Parse(signature);
			}
			if (HasPlatform(reader.Generation))
			{
				TargetPlatform = (Platform)reader.ReadUInt32();
			}

			if (HasEnableTypeTree(reader.Generation))
			{
				EnableTypeTree = reader.ReadBoolean();
			}
			else
			{
				EnableTypeTree = true;
			}

			Types = reader.ReadSerializedTypeArray<SerializedType>(EnableTypeTree);

			if (HasLongFileID(reader.Generation))
			{
				LongFileID = reader.ReadUInt32();
			}

#warning TODO: pass LongFileID to ObjectInfo
			Object = reader.ReadSerializedArray<ObjectInfo>();

			if (HasScriptTypes(reader.Generation))
			{
				ScriptTypes = reader.ReadSerializedArray<LocalSerializedObjectIdentifier>();
			}

			Externals = reader.ReadSerializedArray<FileIdentifier>();

			if (HasRefTypes(reader.Generation))
			{
				RefTypes = reader.ReadSerializedTypeArray<SerializedTypeReference>(EnableTypeTree);
			}
			if (HasUserInformation(reader.Generation))
			{
				UserInformation = reader.ReadStringZeroTerm();
			}
		}

		private void Write(SerializedWriter writer)
		{
			if (HasSignature(writer.Generation))
			{
				writer.WriteStringZeroTerm(UnityVersion.ToString());
			}
			if (HasPlatform(writer.Generation))
			{
				writer.Write((uint)TargetPlatform);
			}
			if (HasEnableTypeTree(writer.Generation))
			{
				writer.Write(EnableTypeTree);
			}

			writer.WriteSerializedTypeArray(Types, EnableTypeTree);
			if (HasLongFileID(writer.Generation))
			{
				writer.Write(LongFileID);
			}

			writer.WriteSerializedArray(Object);
			if (HasScriptTypes(writer.Generation))
			{
				writer.WriteSerializedArray(ScriptTypes);
			}
			writer.WriteSerializedArray(Externals);
			if (HasRefTypes(writer.Generation))
			{
				writer.WriteSerializedTypeArray(RefTypes, EnableTypeTree);
			}
			if (HasUserInformation(writer.Generation))
			{
				writer.WriteStringZeroTerm(UserInformation);
			}
		}

		public UnityVersion UnityVersion { get; set; }
		public Platform TargetPlatform { get; set; }
		public bool EnableTypeTree { get; set; }
		public SerializedType[] Types { get; set; }
		/// <summary>
		/// Indicate that <see cref="ObjectInfo.FileID"/> is 8 bytes size<br/>
		/// Serialized files with this enabled field doesn't exist
		/// </summary>
		public uint LongFileID { get; set; }
		public bool SwapEndianess { get; set; }
		public ObjectInfo[] Object { get; set; }
		public LocalSerializedObjectIdentifier[] ScriptTypes { get; set; }
		public FileIdentifier[] Externals { get; set; }
		public string UserInformation { get; set; }
		public SerializedTypeReference[] RefTypes { get; set; }
	}
}
