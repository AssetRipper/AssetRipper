namespace AssetRipper.IO.Files.SerializedFiles;

/// <summary>
/// SerializedFileFormatVersion in the pdb
/// </summary>
public enum FormatVersion
{
	Unsupported = 1,
	Unknown_2 = 2,
	Unknown_3 = 3,
	/// <summary>
	/// 1.2.0 to 2.0.0
	/// </summary>
	Unknown_5 = 5,
	/// <summary>
	/// 2.1.0 to 2.6.1
	/// </summary>
	Unknown_6 = 6,
	/// <summary>
	/// 3.0.0b
	/// </summary>
	Unknown_7 = 7,
	/// <summary>
	/// 3.0.0 to 3.4.2
	/// </summary>
	Unknown_8 = 8,
	/// <summary>
	/// 3.5.0 to 4.7.2
	/// </summary>
	Unknown_9 = 9,
	/// <summary>
	/// 5.0.0aunk1
	/// </summary>
	Unknown_10 = 10,
	/// <summary>
	/// 5.0.0aunk2
	/// </summary>
	HasScriptTypeIndex = 11,
	/// <summary>
	/// 5.0.0aunk3
	/// </summary>
	Unknown_12 = 12,
	/// <summary>
	/// 5.0.0aunk4
	/// </summary>
	HasTypeTreeHashes = 13,
	/// <summary>
	/// 5.0.0unk
	/// </summary>
	Unknown_14 = 14,
	/// <summary>
	/// 5.0.1 to 5.4.0
	/// </summary>
	SupportsStrippedObject = 15,
	/// <summary>
	/// 5.5.0a
	/// </summary>
	RefactoredClassId = 16,
	/// <summary>
	/// 5.5.0unk to 2018.4
	/// </summary>
	RefactorTypeData = 17,
	/// <summary>
	/// 2019.1a
	/// </summary>
	RefactorShareableTypeTreeData = 18,
	/// <summary>
	/// 2019.1unk
	/// </summary>
	TypeTreeNodeWithTypeFlags = 19,
	/// <summary>
	/// 2019.2
	/// </summary>
	SupportsRefObject = 20,
	/// <summary>
	/// 2019.3 to 2019.4
	/// </summary>
	StoresTypeDependencies = 21,
	/// <summary>
	/// 2020.1 to x
	/// </summary>
	LargeFilesSupport = 22,
}
internal static class FormatVersionExtensions
{
	private static UnityVersion ToDefaultUnityVersion(this FormatVersion generation)
	{
		return generation switch
		{
			< FormatVersion.Unknown_5 => new UnityVersion(1, 2, 2),
			FormatVersion.Unknown_5 => new UnityVersion(1, 6),
			FormatVersion.Unknown_6 => new UnityVersion(2, 5),
			FormatVersion.Unknown_7 => new UnityVersion(3, 0, 0, UnityVersionType.Beta, 1),
			_ => throw new ArgumentOutOfRangeException(nameof(generation)),
		};
	}
}
