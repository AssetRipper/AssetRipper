using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

public abstract class SerializedTypeBase
{
	public int TypeID
	{
		get
		{
			return RawTypeID;
		}
		set
		{
			RawTypeID = value;
		}
	}

	/// <summary>
	/// For versions less than 17, it specifies <see cref="TypeID"/> or -<see cref="ScriptTypeIndex"/> -1 for MonoBehaviour
	/// </summary>
	public int OriginalTypeID
	{
		get
		{
			return RawTypeID;
		}
		set
		{
			RawTypeID = value;
		}
	}
	public int RawTypeID { get; set; }
	public bool IsStrippedType { get; set; }
	/// <summary>
	/// For <see cref="ClassIDType.MonoBehaviour"/> specifies script type
	/// </summary>
	public short ScriptTypeIndex { get; set; }
	/// <summary>
	/// The type of the class.
	/// </summary>
	public TypeTrees.TypeTree OldType { get; } = new();
	/// <summary>
	/// Hash128
	/// </summary>
	public byte[] ScriptID { get; set; } = Array.Empty<byte>();
	public byte[] OldTypeHash { get; set; } = Array.Empty<byte>();

	public void Read(SerializedReader reader, bool hasTypeTree)
	{
		RawTypeID = reader.ReadInt32();
		int typeIdLocal;
		if (reader.Generation < FormatVersion.RefactoredClassId)
		{
			typeIdLocal = RawTypeID < 0 ? -1 : RawTypeID;
			IsStrippedType = false;
			ScriptTypeIndex = -1;
		}
		else
		{
			typeIdLocal = RawTypeID;
			IsStrippedType = reader.ReadBoolean();
		}

		if (reader.Generation >= FormatVersion.RefactorTypeData)
		{
			ScriptTypeIndex = reader.ReadInt16();
		}

		if (reader.Generation >= FormatVersion.HasTypeTreeHashes)
		{
			bool readScriptID = (typeIdLocal == -1)
				|| (typeIdLocal == 114)
				|| (!IgnoreScriptTypeForHash(reader.Generation, reader.Version) && ScriptTypeIndex >= 0);
			if (readScriptID)
			{
				ScriptID = reader.ReadBytes(16);//actually read as 4 uint
			}
			OldTypeHash = reader.ReadBytes(16);//actually read as 4 uint
		}

		if (hasTypeTree)
		{
			OldType.Read(reader);
			if (reader.Generation < FormatVersion.HasTypeTreeHashes)
			{
				//OldTypeHash gets recalculated here in a complicated way on 2023.
			}
			else if (reader.Generation >= FormatVersion.StoresTypeDependencies)
			{
				ReadTypeDependencies(reader);
			}
		}
	}

	protected abstract void ReadTypeDependencies(SerializedReader reader);

	protected abstract bool IgnoreScriptTypeForHash(FormatVersion formatVersion, UnityVersion unityVersion);

	protected abstract void WriteTypeDependencies(SerializedWriter writer);

	public void Write(SerializedWriter writer, bool hasTypeTree)
	{
		writer.Write(RawTypeID);
		if (writer.Generation >= FormatVersion.RefactoredClassId)
		{
			writer.Write(IsStrippedType);
		}
		if (writer.Generation >= FormatVersion.RefactorTypeData)
		{
			writer.Write(ScriptTypeIndex);
		}
		if (writer.Generation >= FormatVersion.HasTypeTreeHashes)
		{
			bool writeScriptID = (RawTypeID == -1)
				|| (RawTypeID == 114)
				|| (!IgnoreScriptTypeForHash(writer.Generation, writer.Version) && ScriptTypeIndex >= 0);
			if (writeScriptID)
			{
				writer.Write(ScriptID);//actually written as 4 uint
			}
			writer.Write(OldTypeHash);//actually written as 4 uint
		}

		if (hasTypeTree)
		{
			OldType.Write(writer);
			if (writer.Generation >= FormatVersion.StoresTypeDependencies)
			{
				WriteTypeDependencies(writer);
			}
		}
	}

	public override string ToString()
	{
		return TypeID.ToString();
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
