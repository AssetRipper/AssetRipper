namespace AssetRipper.Yaml
{
	public sealed class YamlMappingNode : YamlNode
	{
		public YamlMappingNode() { }

		public YamlMappingNode(MappingStyle style)
		{
			Style = style;
		}

		public void Add(int key, long value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(int key, string value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(int key, YamlNode value)
		{
			YamlScalarNode keyNode = new(key);
			InsertEnd(keyNode, value);
		}

		public void Add(uint key, string value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(uint key, YamlNode value)
		{
			YamlScalarNode keyNode = new(key);
			InsertEnd(keyNode, value);
		}

		public void Add(long key, string value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(long key, YamlNode value)
		{
			YamlScalarNode keyNode = new(key);
			InsertEnd(keyNode, value);
		}

		public void Add(string key, bool value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, byte value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, short value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, ushort value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, int value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, uint value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, long value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, ulong value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, float value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, double value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, string value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(string key, YamlNode value)
		{
			YamlScalarNode keyNode = new(key, true);
			InsertEnd(keyNode, value);
		}

		public void Add(YamlNode key, bool value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, byte value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, short value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, ushort value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, int value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, uint value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, long value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, ulong value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, float value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, string value)
		{
			YamlScalarNode valueNode = new(value);
			Add(key, valueNode);
		}

		public void Add(YamlNode key, YamlNode value)
		{
			if (key.NodeType != YamlNodeType.Scalar)
			{
				throw new Exception($"Only {YamlNodeType.Scalar} node as a key supported");
			}

			InsertEnd(key, value);
		}

		public void Append(YamlMappingNode map)
		{
			foreach (KeyValuePair<YamlNode, YamlNode> child in map.m_children)
			{
				Add(child.Key, child.Value);
			}
		}

		public void InsertBegin(string key, int value)
		{
			YamlScalarNode valueNode = new(value);
			InsertBegin(key, valueNode);
		}

		public void InsertBegin(string key, YamlNode value)
		{
			YamlScalarNode keyNode = new(key, true);
			InsertBegin(keyNode, value);
		}

		public void InsertBegin(YamlNode key, YamlNode value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			KeyValuePair<YamlNode, YamlNode> pair = new(key, value);
			m_children.Insert(0, pair);
		}

		internal override void Emit(Emitter emitter)
		{
			base.Emit(emitter);

			StartChildren(emitter);
			foreach (KeyValuePair<YamlNode, YamlNode> kvp in m_children)
			{
				YamlNode key = kvp.Key;
				YamlNode value = kvp.Value;

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

		private void StartTransition(Emitter emitter, YamlNode next)
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

		private void EndTransition(Emitter emitter, YamlNode next)
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

		private void InsertEnd(YamlNode key, YamlNode value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			KeyValuePair<YamlNode, YamlNode> pair = new(key, value);
			m_children.Add(pair);
		}

		public static YamlMappingNode Empty { get; } = new YamlMappingNode(MappingStyle.Flow);

		public override YamlNodeType NodeType => YamlNodeType.Mapping;
		public override bool IsMultiline => Style == MappingStyle.Block && m_children.Count > 0;
		public override bool IsIndent => Style == MappingStyle.Block;

		public MappingStyle Style { get; set; }

		public readonly List<KeyValuePair<YamlNode, YamlNode>> m_children = new();
	}
}
