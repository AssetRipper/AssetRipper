﻿using System.Collections.Generic;

namespace AssetRipper.Yaml
{
	public sealed class YamlSequenceNode : YamlNode
	{
		public YamlSequenceNode() { }

		public YamlSequenceNode(SequenceStyle style)
		{
			Style = style;
		}

		public void Add(bool value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(byte value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(short value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(ushort value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(int value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(uint value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(long value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(ulong value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(float value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(double value)
		{
			YamlScalarNode node = new YamlScalarNode(value, Style.IsRaw());
			Add(node);
		}

		public void Add(string value)
		{
			YamlScalarNode node = new YamlScalarNode(value);
			Add(node);
		}

		public void Add(YamlNode child)
		{
			m_children.Add(child);
		}

		internal override void Emit(Emitter emitter)
		{
			base.Emit(emitter);

			StartChildren(emitter);
			foreach (YamlNode child in m_children)
			{
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
					if (m_children.Count == 0)
						emitter.Write('[');
					break;

				case SequenceStyle.BlockCurve:
					if (m_children.Count == 0)
						emitter.Write('{');
					break;

				case SequenceStyle.Flow:
					emitter.Write('[');
					break;

				case SequenceStyle.Raw:
					if (m_children.Count == 0)
						emitter.Write('[');
					break;
			}
		}

		private void EndChildren(Emitter emitter)
		{
			switch (Style)
			{
				case SequenceStyle.Block:
					if (m_children.Count == 0)
						emitter.Write(']');
					emitter.WriteLine();
					break;

				case SequenceStyle.BlockCurve:
					if (m_children.Count == 0)
						emitter.WriteClose('}');
					emitter.WriteLine();
					break;

				case SequenceStyle.Flow:
					emitter.WriteClose(']');
					break;

				case SequenceStyle.Raw:
					if (m_children.Count == 0)
						emitter.Write(']');
					emitter.WriteLine();
					break;
			}
		}

		private void StartChild(Emitter emitter, YamlNode next)
		{
			if (Style.IsAnyBlock())
			{
				emitter.Write('-').Write(' ');

				if (next.NodeType == NodeType)
					emitter.IncreaseIndent();
			}
			if (next.IsIndent)
				emitter.IncreaseIndent();
		}

		private void EndChild(Emitter emitter, YamlNode next)
		{
			if (Style.IsAnyBlock())
			{
				emitter.WriteLine();
				if (next.NodeType == NodeType)
					emitter.DecreaseIndent();
			}
			else if (Style == SequenceStyle.Flow)
				emitter.WriteSeparator().WriteWhitespace();
			if (next.IsIndent)
				emitter.DecreaseIndent();
		}

		public static YamlSequenceNode Empty { get; } = new YamlSequenceNode();

		public override YamlNodeType NodeType => YamlNodeType.Sequence;
		public override bool IsMultiline => Style.IsAnyBlock() && m_children.Count > 0;
		public override bool IsIndent => false;

		public SequenceStyle Style { get; }

		public readonly List<YamlNode> m_children = new List<YamlNode>();
	}
}
