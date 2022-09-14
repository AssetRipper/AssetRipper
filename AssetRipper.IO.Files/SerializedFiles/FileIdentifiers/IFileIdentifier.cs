namespace AssetRipper.IO.Files.SerializedFiles.FileIdentifiers;

public partial interface IFileIdentifier
{
	/// <summary>
	/// <see cref="PathName"/> without prefixes such as archive:/directory/fileName
	/// </summary>
	string PathNameFixed { get; }

	string GetFilePath();
}
