using uTinyRipper.Classes.Misc;

namespace uTinyRipper.SerializedFiles
{
	public struct SerializedType : ISerializedReadable, ISerializedWritable
	{
		public SerializedType(bool enableTypeTree) :
			this()
		{
			if (enableTypeTree)
			{
				OldType = new TypeTree();
			}
		}
		
		/// <summary>
		/// 5.5.0a and greater
		/// </summary>
		public static bool HasIsStrippedType(FormatVersion generation) => generation >= FormatVersion.RefactoredClassId;
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasScriptTypeIndex(FormatVersion generation) => generation >= FormatVersion.RefactorTypeData;
		/// <summary>
		/// 5.0.0unk2 and greater
		/// </summary>
		public static bool HasHash(FormatVersion generation) => generation >= FormatVersion.HasTypeTreeHashes;
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasTypeDependencies(FormatVersion generation) => generation >= FormatVersion.StoresTypeDependencies;

		public void Read(SerializedReader reader)
		{
			if (HasScriptTypeIndex(reader.Generation))
			{
				TypeID = (ClassIDType)reader.ReadInt32();
			}
			else
			{
				OriginalTypeID = reader.ReadInt32();
			}
			if (HasIsStrippedType(reader.Generation))
			{
				IsStrippedType = reader.ReadBoolean();
			}
			if (HasScriptTypeIndex(reader.Generation))
			{
				ScriptTypeIndex = reader.ReadInt16();
			}

			if (HasHash(reader.Generation))
			{
				if (TypeID == ClassIDType.MonoBehaviour)
				{
					ScriptID.Read(reader);
				}
				OldTypeHash.Read(reader);
			}

			if (OldType != null)
			{
				OldType.Read(reader);
				if (HasTypeDependencies(reader.Generation))
				{
					TypeDependencies = reader.ReadInt32Array();
				}
			}
		}

		public void Write(SerializedWriter writer)
		{
			if (HasScriptTypeIndex(writer.Generation))
			{
				writer.Write((int)TypeID);
			}
			else
			{
				writer.Write(OriginalTypeID);
			}
			if (HasIsStrippedType(writer.Generation))
			{
				writer.Write(IsStrippedType);
			}
			if (HasScriptTypeIndex(writer.Generation))
			{
				writer.Write(ScriptTypeIndex);
			}

			if (HasHash(writer.Generation))
			{
				if (TypeID == ClassIDType.MonoBehaviour)
				{
					ScriptID.Write(writer);
				}
				OldTypeHash.Write(writer);
			}

			if (OldType != null)
			{
				OldType.Write(writer);
				if (HasTypeDependencies(writer.Generation))
				{
					writer.WriteArray(TypeDependencies);
				}
			}
		}

		public override string ToString()
		{
			return TypeID.ToString();
		}

		/// <summary>
		/// For old version it specifies <see cref="TypeID"/> or -<see cref="ScriptTypeIndex"/> for MonoBehaviour
		/// </summary>
		public int OriginalTypeID
		{
			get
			{
				return TypeID == ClassIDType.MonoBehaviour ? -(ScriptTypeIndex + 1) : (int)TypeID;
			}
			set
			{
				if (value >= 0)
				{
					TypeID = (ClassIDType)value;
					ScriptTypeIndex = -1;
				}
				else
				{
					TypeID = ClassIDType.MonoBehaviour;
					ScriptTypeIndex = (short)(-value - 1);
				}
			}
		}
		public ClassIDType TypeID { get; set; }
		public bool IsStrippedType { get; set; }
		/// <summary>
		/// For <see cref="ClassIDType.MonoBehaviour"/> specifies script type
		/// </summary>
		public short ScriptTypeIndex { get; set; }
		/// <summary>
		/// The type of the class.
		/// </summary>
		public TypeTree OldType { get; set; }
		public int[] TypeDependencies { get; set; }

		public Hash128 ScriptID;
		public Hash128 OldTypeHash;
	}
}
