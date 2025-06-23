namespace AssetRipper.IO.Files.BundleFiles;

public enum BundleVersion
{
	Unknown = 0,

	BF_100_250 = 1,
	BF_260_340 = 2,
	BF_350_4x = 3,
	BF_520a1 = 4,
	BF_520aunk = 5,
	BF_520_x = 6,
	/// <summary>
	/// Several 4-byte integers were upgraded to 8-byte integers in order to support files larger than 2 GB.
	/// </summary>
	BF_LargeFilesSupport = 7,
	/// <summary>
	/// This seems to be exactly the same as <see cref="BF_LargeFilesSupport"/>.
	/// </summary>
	BF_2022_2 = 8,
}
