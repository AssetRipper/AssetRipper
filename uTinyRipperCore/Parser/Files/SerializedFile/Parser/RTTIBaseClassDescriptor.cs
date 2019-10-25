using uTinyRipper.Classes;

namespace uTinyRipper.SerializedFiles
{
	internal sealed class RTTIBaseClassDescriptor : ISerializedFileReadable
	{
		public RTTIBaseClassDescriptor(bool isSerializeTypeTrees)
		{
			if (isSerializeTypeTrees)
			{
				Tree = new TypeTree();
			}
		}
		
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadScriptType(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_550_2018;
		}
		/// <summary>
		/// 5.0.0unk2 and greater
		/// </summary>
		public static bool IsReadHash(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_500aunk2;
		}

		public void Read(SerializedFileReader reader)
		{
			ClassID = (ClassIDType)reader.ReadInt32();
			if (IsReadScriptType(reader.Generation))
			{
				IsStrippedType = reader.ReadBoolean();
				ScriptID = reader.ReadInt16();
			}
			else
			{
				// For old version it specifies ClassIDType or -ScriptID for MonoBehaviour
				int uniqueTypeID = (int)ClassID;
				if (uniqueTypeID < 0)
				{
					ClassID = ClassIDType.MonoBehaviour;
					ScriptID = (short)(-uniqueTypeID - 1);
				}
			}

			if (IsReadHash(reader.Generation))
			{
				if (ClassID == ClassIDType.MonoBehaviour)
				{
					ScriptHash.Read(reader);
				}
				TypeHash.Read(reader);
			}
			
			// isSerializeTypeTrees
			if (Tree != null)
			{
				Tree.Read(reader);
			}
		}

		public override string ToString()
		{
			return ClassID.ToString();
		}

		public ClassIDType ClassID { get; private set; }
		public bool IsStrippedType { get; private set; }
		/// <summary>
		/// For MonoBehaviours specifies script type
		/// </summary>
		public short ScriptID { get; private set; }
		/// <summary>
		/// The type of the class.
		/// </summary>
		public TypeTree Tree { get; }

		public Hash128 ScriptHash;
		public Hash128 TypeHash;
	}
}
