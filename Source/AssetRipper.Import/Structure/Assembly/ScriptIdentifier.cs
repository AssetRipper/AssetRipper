namespace AssetRipper.Import.Structure.Assembly;

public readonly record struct ScriptIdentifier
{
	public ScriptIdentifier(string assembly, string @namespace, string name)
	{
		ArgumentException.ThrowIfNullOrEmpty(assembly);
		ArgumentNullException.ThrowIfNull(@namespace);
		ArgumentException.ThrowIfNullOrEmpty(name);
		Assembly = assembly;
		Namespace = @namespace;
		Name = name;
	}

	public static string ToUniqueName(string assembly, string @namespace, string name)
	{
		return @namespace == string.Empty ? $"[{assembly}]{name}" : $"[{assembly}]{@namespace}.{name}";
	}

	public static string ToUniqueName(string assembly, string fullName)
	{
		return $"[{assembly}]{fullName}";
	}

	public override string? ToString()
	{
		return IsDefault ? base.ToString() : Namespace == string.Empty ? $"{Name}" : $"{Namespace}.{Name}";
	}

	public bool IsDefault => Name == null;
	public string UniqueName => ToUniqueName(Assembly, Namespace, Name);

	public string Assembly { get; }
	public string Namespace { get; }
	public string Name { get; }
}
