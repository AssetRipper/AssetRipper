namespace AssetRipper.Utils.SourceGeneration;

public static class IndentedTextWriterFactory
{
	/// <summary>
	/// Make a new <see cref="IndentedTextWriter"/>.
	/// </summary>
	/// <remarks>
	/// This writer uses <see cref="StreamWriter.AutoFlush"/>, LF line endings, and tab indentation.
	/// </remarks>
	/// <param name="directoryPath">The target directory of the file to create.</param>
	/// <param name="fileName">The name of the target file without any file extension.</param>
	/// <returns>An <see cref="IndentedTextWriter"/> for a newly created file at the location specified.</returns>
	public static IndentedTextWriter Create(string directoryPath, string fileName)
	{
		string path = Path.Combine(directoryPath, $"{fileName}.g.cs");
		FileStream stream = File.Create(path);
		StreamWriter streamWriter = new StreamWriter(stream, leaveOpen: false);
		streamWriter.NewLine = "\n";
		streamWriter.AutoFlush = true;
		return new IndentedTextWriter(streamWriter, "\t");
	}
}
