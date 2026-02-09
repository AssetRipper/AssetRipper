namespace AssetRipper.Yaml;

public abstract class YamlNode
{
	internal virtual void Emit(Emitter emitter)
	{
		bool isWrote = false;
		if (!CustomTag.IsEmpty)
		{
			emitter.Write(CustomTag.ToString()).WriteWhitespace();
			isWrote = true;
		}
		if (Anchor.Length > 0)
		{
			emitter.Write("&").Write(Anchor).WriteWhitespace();
			isWrote = true;
		}
		if (Stripped)
		{
			emitter.Write("stripped").WriteWhitespace();
			isWrote = true;
		}

		if (isWrote)
		{
			if (IsMultiline)
			{
				emitter.WriteLine();
			}
		}
	}

	public string EmitToString(bool formatKeys = false)
	{
		using StringWriter writer = new();
		Emitter emitter = new(writer, formatKeys);
		Emit(emitter);
		return writer.ToString();
	}

	public abstract YamlNodeType NodeType { get; }
	public abstract bool IsMultiline { get; }
	public abstract bool IsIndent { get; }

	public string Tag
	{
		get => CustomTag.Content;
		set => CustomTag = new YamlTag(YamlWriter.DefaultTagHandle, value);
	}
	public YamlTag CustomTag { get; set; }
	public string Anchor { get; set; } = string.Empty;
	public bool Stripped { get; set; }
}
