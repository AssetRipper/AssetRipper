using System.Collections.Generic;

namespace UtinyRipper.Exporter.YAML
{
	public sealed class YAMLSequenceNode : YAMLNode
	{
		public YAMLSequenceNode()
		{
		}

		public YAMLSequenceNode(SequenceStyle style)
		{
			Style = style;
		}

		public void Add(bool value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(byte value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(short value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(ushort value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(int value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(uint value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(long value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(ulong value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(float value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(string value)
		{
			YAMLScalarNode node = new YAMLScalarNode(value);
			node.Style = Style.ToScalarStyle();
			Add(node);
		}

		public void Add(YAMLNode child)
		{
			m_children.Add(child);
		}
		
		internal override void Emit(Emitter emitter)
		{
			base.Emit(emitter);

			StartChildren(emitter);
			foreach (YAMLNode child in m_children)
			{
				StartChild(emitter, child);
				child.Emit(emitter);
				EndChild(emitter, child);
			}
			EndChildren(emitter);
		}

		private void StartChildren(Emitter emitter)
		{
			if(Style == SequenceStyle.Block)
			{
				if(m_children.Count == 0)
				{
					emitter.Write('[');
				}
			}
			else if (Style == SequenceStyle.Flow)
			{
				emitter.Write('[');
			}
			else if (Style == SequenceStyle.Raw)
			{
				if (m_children.Count == 0)
				{
					emitter.Write('[');
				}
			}
		}

		private void EndChildren(Emitter emitter)
		{
			if (Style == SequenceStyle.Block)
			{
				if (m_children.Count == 0)
				{
					emitter.Write(']');
				}
				emitter.WriteLine();
			}
			else if (Style == SequenceStyle.Flow)
			{
				emitter.WriteClose(']');
			}
			else if (Style == SequenceStyle.Raw)
			{
				if (m_children.Count == 0)
				{
					emitter.Write(']');
				}
				emitter.WriteLine();
			}
		}

		private void StartChild(Emitter emitter, YAMLNode next)
		{
			if(Style == SequenceStyle.Block)
			{
				emitter.Write('-').WriteWhitespace();

				if(next.NodeType == NodeType)
				{
					emitter.IncreaseIntent();
				}
			}
			if(next.IsIndent)
			{
				emitter.IncreaseIntent();
			}
		}

		private void EndChild(Emitter emitter, YAMLNode next)
		{
			if(Style == SequenceStyle.Block)
			{
				emitter.WriteLine();
				if(next.NodeType == NodeType)
				{
					emitter.DecreaseIntent();
				}
			}
			else if(Style == SequenceStyle.Flow)
			{
				emitter.WriteSeparator().WriteWhitespace();
			}
			if (next.IsIndent)
			{
				emitter.DecreaseIntent();
			}
		}

		public static YAMLSequenceNode Empty { get; } = new YAMLSequenceNode();

		public override YAMLNodeType NodeType => YAMLNodeType.Sequence;
		public override bool IsMultyline
		{
			get
			{
				return Style == SequenceStyle.Block && m_children.Count > 0;
			}
		}
		public override bool IsIndent => false;

		public SequenceStyle Style { get; set; }

		private readonly List<YAMLNode> m_children = new List<YAMLNode>();
	}
}
