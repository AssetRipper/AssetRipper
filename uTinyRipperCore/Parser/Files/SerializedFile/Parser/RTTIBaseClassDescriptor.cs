using uTinyRipper.Classes.Misc;

namespace uTinyRipper.SerializedFiles
{
	public struct RTTIBaseClassDescriptor : ISerializedReadable, ISerializedWritable
	{
		public RTTIBaseClassDescriptor(bool isSerializeTypeTrees) :
			this()
		{
			if (isSerializeTypeTrees)
			{
				Tree = new TypeTree();
			}
		}
		
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasScriptType(FileGeneration generation) => generation >= FileGeneration.FG_550_2018;
		/// <summary>
		/// 5.0.0unk2 and greater
		/// </summary>
		public static bool HasHash(FileGeneration generation) => generation >= FileGeneration.FG_500aunk2;

		public void Read(SerializedReader reader)
		{
			if (HasScriptType(reader.Generation))
			{
				ClassID = (ClassIDType)reader.ReadInt32();
				IsStrippedType = reader.ReadBoolean();
				ScriptID = reader.ReadInt16();
			}
			else
			{
				UniqueTypeID = reader.ReadInt32();
			}

			if (HasHash(reader.Generation))
			{
				if (ClassID == ClassIDType.MonoBehaviour)
				{
					ScriptHash.Read(reader);
				}
				TypeHash.Read(reader);
			}

			// isSerializeTypeTrees
			Tree?.Read(reader);
		}

		public void Write(SerializedWriter writer)
		{
			if (HasScriptType(writer.Generation))
			{
				writer.Write((int)ClassID);
				writer.Write(IsStrippedType);
				writer.Write(ScriptID);
			}
			else
			{
				writer.Write(UniqueTypeID);
			}

			if (HasHash(writer.Generation))
			{
				if (ClassID == ClassIDType.MonoBehaviour)
				{
					ScriptHash.Write(writer);
				}
				TypeHash.Write(writer);
			}

			// isSerializeTypeTrees
			Tree?.Write(writer);
		}

		public override string ToString()
		{
			return ClassID.ToString();
		}

		public ClassIDType ClassID { get; set; }
		public bool IsStrippedType { get; set; }
		/// <summary>
		/// For MonoBehaviours specifies script type
		/// </summary>
		public short ScriptID { get; set; }
		/// <summary>
		/// The type of the class.
		/// </summary>
		public TypeTree Tree { get; set; }

		/// <summary>
		/// For old version it specifies ClassIDType or -ScriptID for MonoBehaviour
		/// </summary>
		public int UniqueTypeID
		{
			get
			{
				return ClassID == ClassIDType.MonoBehaviour ? -(ScriptID + 1) : (int)ClassID;
			}
			set
			{
				if (value >= 0)
				{
					ClassID = (ClassIDType)value;
					ScriptID = 0;
				}
				else
				{
					ClassID = ClassIDType.MonoBehaviour;
					ScriptID = (short)(-value - 1);
				}
			}
		}

		public Hash128 ScriptHash;
		public Hash128 TypeHash;
	}
}
