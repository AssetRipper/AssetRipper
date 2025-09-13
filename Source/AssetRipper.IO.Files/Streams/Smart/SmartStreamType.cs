namespace AssetRipper.IO.Files.Streams.Smart;

public enum SmartStreamType
{
	/// <summary>
	/// The <see cref="SmartStream"/> is not backed by a <see cref="Stream"/>.
	/// </summary>
	Null,
	/// <summary>
	/// The <see cref="SmartStream"/> is backed by a <see cref="FileStream"/> or <see cref="MultiFileStream"/>.
	/// </summary>
	File,
	/// <summary>
	/// The <see cref="SmartStream"/> is backed by a <see cref="MemoryStream"/>.
	/// </summary>
	Memory,
}
