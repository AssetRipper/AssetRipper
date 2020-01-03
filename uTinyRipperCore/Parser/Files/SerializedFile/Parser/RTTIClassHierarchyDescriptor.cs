namespace uTinyRipper.SerializedFiles
{
	public struct RTTIClassHierarchyDescriptor : ISerializedReadable, ISerializedWritable
	{
		/// <summary>
		/// 3.0.0b and greater
		/// </summary>
		public static bool HasSignature(FileGeneration generation) => generation > FileGeneration.FG_300b;
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasPlatform(FileGeneration generation) => generation >= FileGeneration.FG_300_342;
		/// <summary>
		/// 5.0.0Unk2 and greater
		/// </summary>
		public static bool HasSerializeTypeTrees(FileGeneration generation) => generation >= FileGeneration.FG_500aunk2;
		/// <summary>
		/// 3.0.0b to 4.x.x
		/// </summary>
		public static bool HasUnknown(FileGeneration generation) => generation >= FileGeneration.FG_300b && generation < FileGeneration.FG_500;

		public void Read(SerializedReader reader)
		{
			if (HasSignature(reader.Generation))
			{
				string signature = reader.ReadStringZeroTerm();
				Version = Version.Parse(signature);
			}
			if (HasPlatform(reader.Generation))
			{
				Platform = (Platform)reader.ReadUInt32();
			}

			bool serializeTypeTree;
			if (HasSerializeTypeTrees(reader.Generation))
			{
				SerializeTypeTrees = reader.ReadBoolean();
				serializeTypeTree = SerializeTypeTrees;
			}
			else
			{
				serializeTypeTree = true;
			}

			Types = reader.ReadSerializedArray(() => new RTTIBaseClassDescriptor(serializeTypeTree));
			if (HasUnknown(reader.Generation))
			{
				Unknown = reader.ReadInt32();
			}
		}

		public void Write(SerializedWriter writer)
		{
			if (HasSignature(writer.Generation))
			{
				writer.WriteStringZeroTerm(Version.ToString());
			}
			if (HasPlatform(writer.Generation))
			{
				writer.Write((uint)Platform);
			}
			if (HasSerializeTypeTrees(writer.Generation))
			{
				writer.Write(SerializeTypeTrees);
			}

			writer.WriteSerializedArray(Types);
			if (HasUnknown(writer.Generation))
			{
				writer.Write(Unknown);
			}
		}

		/// <summary>
		/// Signature
		/// </summary>
		public Version Version { get; set; }
		/// <summary>
		/// Attributes
		/// </summary>
		public Platform Platform { get; set; }
		public bool SerializeTypeTrees { get; set; }
		public RTTIBaseClassDescriptor[] Types { get; set; }
		public int Unknown { get; set; }

		public const int HierarchyMinSize = 4;
	}
}
