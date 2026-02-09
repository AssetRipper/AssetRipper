namespace AssetRipper.Yaml;

public sealed class YamlSequenceNode : YamlNode
{
	public YamlSequenceNode() { }

	public YamlSequenceNode(SequenceStyle style)
	{
		Style = style;
	}

	public void Add(bool value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(byte value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(short value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(ushort value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(int value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(uint value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(long value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(ulong value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(float value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(double value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(string value)
	{
		YamlScalarNode node = YamlScalarNode.Create(value);
		Add(node);
	}

	public void Add(YamlNode child)
	{
		Children.Add(child);
	}

	internal override void Emit(Emitter emitter)
	{
		base.Emit(emitter);

		StartChildren(emitter);
		for (int i = 0; i < Children.Count; i++)
		{
			YamlNode child = Children[i];
			StartChild(emitter, child);
			child.Emit(emitter);
			EndChild(emitter, child);
		}
		EndChildren(emitter);
	}

	private void StartChildren(Emitter emitter)
	{
		switch (Style)
		{
			case SequenceStyle.Block:
				if (Children.Count == 0)
				{
					emitter.Write('[');
				}

				break;

			case SequenceStyle.BlockCurve:
				if (Children.Count == 0)
				{
					emitter.Write('{');
				}

				break;

			case SequenceStyle.Flow:
				emitter.Write('[');
				break;
		}
	}

	private void EndChildren(Emitter emitter)
	{
		switch (Style)
		{
			case SequenceStyle.Block:
				if (Children.Count == 0)
				{
					emitter.Write(']');
				}

				emitter.WriteLine();
				break;

			case SequenceStyle.BlockCurve:
				if (Children.Count == 0)
				{
					emitter.WriteClose('}');
				}

				emitter.WriteLine();
				break;

			case SequenceStyle.Flow:
				emitter.WriteClose(']');
				break;
		}
	}

	private void StartChild(Emitter emitter, YamlNode next)
	{
		if (Style.IsAnyBlock())
		{
			emitter.Write('-').Write(' ');

			if (next.NodeType == NodeType)
			{
				emitter.IncreaseIndent();
			}
		}
		if (next.IsIndent)
		{
			emitter.IncreaseIndent();
		}
	}

	private void EndChild(Emitter emitter, YamlNode next)
	{
		if (Style.IsAnyBlock())
		{
			emitter.WriteLine();
			if (next.NodeType == NodeType)
			{
				emitter.DecreaseIndent();
			}
		}
		else if (Style == SequenceStyle.Flow)
		{
			emitter.WriteSeparator().WriteWhitespace();
		}

		if (next.IsIndent)
		{
			emitter.DecreaseIndent();
		}
	}

	public override YamlNodeType NodeType => YamlNodeType.Sequence;
	public override bool IsMultiline => Style.IsAnyBlock() && Children.Count > 0;
	public override bool IsIndent => false;

	public SequenceStyle Style { get; }

	public List<YamlNode> Children { get; } = new();

	public override string ToString()
	{
		return $"Count = {Children.Count}";
	}
}
