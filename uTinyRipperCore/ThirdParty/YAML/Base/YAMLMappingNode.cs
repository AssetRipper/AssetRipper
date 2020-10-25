using System;
using System.Collections.Generic;

namespace uTinyRipper.YAML
{
	public sealed class YAMLMappingNode : YAMLNode
	{
		public YAMLMappingNode()
		{
		}

		public YAMLMappingNode(MappingStyle style)
		{
			Style = style;
		}

		public void Add(int key, long value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(int key, string value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(int key, YAMLNode value)
		{
			YAMLScalarNode keyNode = new YAMLScalarNode(key);
			InsertEnd(keyNode, value);
		}

		public void Add(uint key, string value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(uint key, YAMLNode value)
		{
			YAMLScalarNode keyNode = new YAMLScalarNode(key);
			InsertEnd(keyNode, value);
		}

		public void Add(long key, string value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(long key, YAMLNode value)
		{
			YAMLScalarNode keyNode = new YAMLScalarNode(key);
			InsertEnd(keyNode, value);
		}

		public void Add(string key, bool value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, byte value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, short value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, ushort value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, int value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, uint value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, long value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, ulong value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, float value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, string value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(string key, YAMLNode value)
		{
			YAMLScalarNode keyNode = new YAMLScalarNode(key, true);
			InsertEnd(keyNode, value);
		}

		public void Add(YAMLNode key, bool value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, byte value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, short value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, ushort value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, int value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, uint value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, long value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, ulong value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, float value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, string value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			Add(key, valueNode);
		}

		public void Add(YAMLNode key, YAMLNode value)
		{
			if (key.NodeType != YAMLNodeType.Scalar)
			{
				throw new Exception($"Only {YAMLNodeType.Scalar} node as a key supported");
			}

			InsertEnd(key, value);
		}

		public void Append(YAMLMappingNode map)
		{
			foreach (KeyValuePair<YAMLNode, YAMLNode> child in map.m_children)
			{
				Add(child.Key, child.Value);
			}
		}

		public void InsertBegin(string key, int value)
		{
			YAMLScalarNode valueNode = new YAMLScalarNode(value);
			InsertBegin(key, valueNode);
		}

		public void InsertBegin(string key, YAMLNode value)
		{
			YAMLScalarNode keyNode = new YAMLScalarNode(key, true);
			InsertBegin(keyNode, value);
		}

		public void InsertBegin(YAMLNode key, YAMLNode value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			KeyValuePair<YAMLNode, YAMLNode> pair = new KeyValuePair<YAMLNode, YAMLNode>(key, value);
			m_children.Insert(0, pair);
		}

		internal override void Emit(Emitter emitter)
		{
			base.Emit(emitter);

			StartChildren(emitter);
			foreach (var kvp in m_children)
			{
				YAMLNode key = kvp.Key;
				YAMLNode value = kvp.Value;

				bool iskey = emitter.IsKey;
				emitter.IsKey = true;
				key.Emit(emitter);
				emitter.IsKey = false;
				StartTransition(emitter, value);
				value.Emit(emitter);
				EndTransition(emitter, value);
				emitter.IsKey = iskey;
			}
			EndChildren(emitter);
		}

		private void StartChildren(Emitter emitter)
		{
			if (Style == MappingStyle.Block)
			{
				if (m_children.Count == 0)
				{
					emitter.Write('{');
				}
			}
			else if (Style == MappingStyle.Flow)
			{
				emitter.Write('{');
			}
		}

		private void EndChildren(Emitter emitter)
		{
			if (Style == MappingStyle.Block)
			{
				if (m_children.Count == 0)
				{
					emitter.Write('}');
				}
				emitter.WriteLine();
			}
			else if (Style == MappingStyle.Flow)
			{
				emitter.WriteClose('}');
			}
		}

		private void StartTransition(Emitter emitter, YAMLNode next)
		{
			emitter.Write(':').WriteWhitespace();
			if (Style == MappingStyle.Block)
			{
				if (next.IsMultiline)
				{
					emitter.WriteLine();
				}
			}
			if (next.IsIndent)
			{
				emitter.IncreaseIndent();
			}
		}

		private void EndTransition(Emitter emitter, YAMLNode next)
		{
			if (Style == MappingStyle.Block)
			{
				emitter.WriteLine();
			}
			else if (Style == MappingStyle.Flow)
			{
				emitter.WriteSeparator().WriteWhitespace();
			}
			if (next.IsIndent)
			{
				emitter.DecreaseIndent();
			}
		}

		private void InsertEnd(YAMLNode key, YAMLNode value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			KeyValuePair<YAMLNode, YAMLNode> pair = new KeyValuePair<YAMLNode, YAMLNode>(key, value);
			m_children.Add(pair);
		}

		public static YAMLMappingNode Empty { get; } = new YAMLMappingNode(MappingStyle.Flow);

		public override YAMLNodeType NodeType => YAMLNodeType.Mapping;
		public override bool IsMultiline => Style == MappingStyle.Block && m_children.Count > 0;
		public override bool IsIndent => Style == MappingStyle.Block;

		public MappingStyle Style { get; set; }

		private readonly List<KeyValuePair<YAMLNode, YAMLNode>> m_children = new List<KeyValuePair<YAMLNode, YAMLNode>>();
	}
}
