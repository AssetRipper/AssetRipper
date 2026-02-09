namespace AssetRipper.Yaml;

public sealed class YamlDocument
{
	public YamlDocument() { }

	public YamlSequenceNode CreateSequenceRoot()
	{
		YamlSequenceNode root = new();
		Root = root;
		return root;
	}

	public YamlMappingNode CreateMappingRoot()
	{
		YamlMappingNode root = new();
		Root = root;
		return root;
	}

	internal void Emit(Emitter emitter, bool isSeparator)
	{
		if (isSeparator)
		{
			emitter.Write("---").WriteWhitespace();
		}

		ThrowIfNullRoot();
		Root.Emit(emitter);
	}

	[MemberNotNull(nameof(Root))]
	private void ThrowIfNullRoot()
	{
		if (Root is null)
		{
			throw new NullReferenceException("Root cannot be null here");
		}
	}

	public YamlNode? Root { get; private set; }
}
