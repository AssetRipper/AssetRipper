using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files.SerializedFiles.IO;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.Parser
{
	public abstract class SerializedTypeBase
	{
		public ClassIDType TypeID { get; set; }
		public bool IsStrippedType { get; set; }
		/// <summary>
		/// For <see cref="ClassIDType.MonoBehaviour"/> specifies script type
		/// </summary>
		public short ScriptTypeIndex { get; set; }
		/// <summary>
		/// The type of the class.
		/// </summary>
		public TypeTree.TypeTree OldType { get; } = new();
		public Hash128 ScriptID { get; } = new();
		public Hash128 OldTypeHash { get; } = new();

		public virtual void Read(SerializedReader reader, bool hasTypeTree)
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
		}

		public virtual void Write(SerializedWriter writer, bool hasTypeTree)
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
		}

		public override string ToString()
		{
			return TypeID.ToString();
		}

		/// <summary>
		/// For versions less than 17, it specifies <see cref="TypeID"/> or -<see cref="ScriptTypeIndex"/> -1 for MonoBehaviour
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

		/// <summary>
		/// 5.5.0a and greater, ie format version 16+
		/// </summary>
		public static bool HasIsStrippedType(FormatVersion generation) => generation >= FormatVersion.RefactoredClassId;
		/// <summary>
		/// 5.5.0 and greater, ie format version 17+
		/// </summary>
		public static bool HasScriptTypeIndex(FormatVersion generation) => generation >= FormatVersion.RefactorTypeData;
		/// <summary>
		/// 5.0.0unk2 and greater, ie format version 13+
		/// </summary>
		public static bool HasHash(FormatVersion generation) => generation >= FormatVersion.HasTypeTreeHashes;
		/// <summary>
		/// 2019.3 and greater, ie format version 21+
		/// </summary>
		public static bool HasTypeDependencies(FormatVersion generation) => generation >= FormatVersion.StoresTypeDependencies;
	}
}
