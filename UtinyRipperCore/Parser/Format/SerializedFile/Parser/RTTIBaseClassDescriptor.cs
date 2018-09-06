using UtinyRipper.Classes;

namespace UtinyRipper.SerializedFiles
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
		public static bool IsReadUnknown(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_550_x;
		}
		/// <summary>
		/// 5.0.0unk2 and greater
		/// </summary>
		public static bool IsReadHash(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_500aunk2;
		}

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsOldHashType(FileGeneration generation)
		{
			return generation < FileGeneration.FG_550_x;
		}

		public void Read(SerializedFileReader reader)
		{
			ClassID = (ClassIDType)reader.ReadInt32();
			if (IsReadUnknown(reader.Generation))
			{
				Unknown = reader.ReadByte();
				ScriptID = reader.ReadInt16();
			}

			if (IsReadHash(reader.Generation))
			{
				if (IsOldHashType(reader.Generation))
				{
					if ((int)ClassID <= -1)
					{
						ScriptHash.Read(reader);
					}
				}
				else
				{
					if (ClassID == ClassIDType.MonoBehaviour)
					{
						ScriptHash.Read(reader);
					}
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

		/// <summary>
		/// The ID of the class. Only one type tree per class ID is allowed. Can be used as a key for a map.
		/// </summary>
		public ClassIDType ClassID { get; private set; }
		public byte Unknown { get; private set; }
		public short ScriptID { get; private set; }
		/// <summary>
		/// The type of the class.
		/// </summary>
		public TypeTree Tree { get; }

		public Hash128 ScriptHash;
		public Hash128 TypeHash;
	}
}
