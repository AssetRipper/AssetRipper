using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files;

public interface IScheme
{
	/// <summary>
	/// Checks if the file can be read by this scheme.
	/// </summary>
	/// <remarks>
	/// Implementations are expected to reset the <paramref name="stream"/> to its initial position.
	/// </remarks>
	/// <param name="stream">The stream for the file.</param>
	/// <returns>True if the file can be read.</returns>
	bool CanRead(SmartStream stream);
	FileBase Read(SmartStream stream, string filePath, string fileName);
}
