using System.IO;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedFileMetadata
	{
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasEndian(FileGeneration generation) => generation <= FileGeneration.FG_300_342;
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool IsMetadataAtTheEnd(FileGeneration generation) => generation <= FileGeneration.FG_300_342;

		/// <summary>
		/// 5.0.0Unk0 and greater
		/// </summary>
		public static bool HasPreload(FileGeneration generation) => generation >= FileGeneration.FG_500aunk;
		/// <summary>
		/// 1.2.0 and greater
		/// </summary>
		public static bool HasUnknown(FileGeneration generation) => generation >= FileGeneration.FG_120_200;

		public void Read(Stream stream, SerializedFileHeader header)
		{
			bool swapEndianess = header.SwapEndianess;
			if (HasEndian(header.Generation))
			{
				SwapEndianess = stream.ReadByte() != 0;
				swapEndianess = SwapEndianess;
			}
			EndianType endianess = swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			using (SerializedReader reader = new SerializedReader(stream, endianess, header.Generation))
			{
				Read(reader);
			}
		}

		public void Write(Stream stream, SerializedFileHeader header)
		{
			bool swapEndianess = header.SwapEndianess;
			if (HasEndian(header.Generation))
			{
				SwapEndianess = stream.ReadByte() != 0;
				swapEndianess = SwapEndianess;
			}
			EndianType endianess = swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			using (SerializedWriter writer = new SerializedWriter(stream, endianess, header.Generation))
			{
				Write(writer);
			}
		}

		private void Read(SerializedReader reader)
		{
			Hierarchy.Read(reader);
			Entries = reader.ReadSerializedArray<AssetEntry>();
			if (HasPreload(reader.Generation))
			{
				Preloads = reader.ReadSerializedArray<ObjectPtr>();
			}
			Dependencies = reader.ReadSerializedArray<FileIdentifier>();
			if (HasUnknown(reader.Generation))
			{
				Unknown = reader.ReadStringZeroTerm();
			}
		}

		private void Write(SerializedWriter writer)
		{
			if (HasEndian(writer.Generation))
			{
				writer.Write(SwapEndianess);
			}
			Hierarchy.Write(writer);
			writer.WriteSerializedArray(Entries);
			if (HasPreload(writer.Generation))
			{
				writer.WriteSerializedArray(Preloads);
			}
			writer.WriteSerializedArray(Dependencies);
			if (HasUnknown(writer.Generation))
			{
				writer.WriteStringZeroTerm(Unknown);
			}
		}

		public bool SwapEndianess { get; set; }
		public AssetEntry[] Entries { get; set; }
		public ObjectPtr[] Preloads { get; set; }
		public FileIdentifier[] Dependencies { get; set; }
		public string Unknown { get; set; }

		public const int MetadataMinSize = RTTIClassHierarchyDescriptor.HierarchyMinSize + 12;

		public RTTIClassHierarchyDescriptor Hierarchy;
	}
}
