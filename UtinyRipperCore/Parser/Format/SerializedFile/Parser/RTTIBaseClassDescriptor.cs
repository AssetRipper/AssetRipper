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

		public void Read(SerializedFileStream stream)
		{
			ClassID = (ClassIDType)stream.ReadInt32();
			if (IsReadUnknown(stream.Generation))
			{
				Unknown = stream.ReadByte();
				ScriptID = stream.ReadInt16();
			}

			if (IsReadHash(stream.Generation))
			{
				if (IsOldHashType(stream.Generation))
				{
					if ((int)ClassID <= -1)
					{
						ScriptHash.Read(stream);
					}
				}
				else
				{
					if (ClassID == ClassIDType.MonoBehaviour)
					{
						ScriptHash.Read(stream);
					}
				}
				TypeHash.Read(stream);
			}
			
			// isSerializeTypeTrees
			if (Tree != null)
			{
				Tree.Read(stream);
			}
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
