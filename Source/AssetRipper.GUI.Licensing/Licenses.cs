using System.Reflection;

namespace AssetRipper.GUI.Licensing;

public static class Licenses
{
	private const string FilePrefix = "AssetRipper.GUI.Licensing.";

	private static Assembly Assembly => typeof(Licenses).Assembly;

	public static IReadOnlyList<string> Names { get; } = Assembly
		.GetManifestResourceNames()
		.Select(t => t.Substring(FilePrefix.Length, t.Length - FilePrefix.Length - 3))
		.ToArray();

	/// <summary>
	/// Load a license file from the embedded resources.
	/// </summary>
	/// <param name="fileName">The name of the file without any extension.</param>
	/// <returns>The loaded text.</returns>
	public static string Load(string fileName)
	{
		if (TryLoad(fileName, out string? license))
		{
			return license;
		}
		throw new LicenseNotFoundException(fileName);
	}

	public static bool TryLoad(string fileName, [NotNullWhen(true)] out string? license)
	{
		using Stream? stream = Assembly.GetManifestResourceStream(FilePrefix + fileName + ".md");
		if (stream is null)
		{
			license = null;
			return false;
		}

		license = new StreamReader(stream).ReadToEnd();
		return true;
	}
}
