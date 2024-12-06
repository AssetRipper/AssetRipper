namespace AssetRipper.IO.Files.SerializedFiles.FileIdentifiers;

public partial record class FileIdentifier_1
{
	public string PathNameFixed { get; private set; } = "";

	partial void OnPathNameAssignment(string value)
	{
		PathNameFixed = SpecialFileNames.FixFileIdentifier(value);
	}

	public string GetFilePath()
	{
		if (Type == AssetType.Meta)
		{
			return Guid.ToString();
		}
		return PathNameFixed;
	}
}
