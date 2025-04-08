namespace AssetRipper.GUI.Licensing;

public static partial class Licenses
{
	/// <summary>
	/// Load a license file.
	/// </summary>
	/// <param name="name">The name of the license (without any extension).</param>
	/// <returns>The loaded text.</returns>
	public static string Load(string name)
	{
		if (TryLoad(name, out string? license))
		{
			return license;
		}
		throw new LicenseNotFoundException(name);
	}

	public static bool TryLoad(string name, [NotNullWhen(true)] out string? license)
	{
		license = TryLoad(name);
		return license is not null;
	}
}
