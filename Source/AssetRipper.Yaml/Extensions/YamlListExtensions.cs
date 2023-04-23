using System.Text;

namespace AssetRipper.Yaml.Extensions
{
	public static class YamlListExtensions
	{
		public static YamlNode ExportYaml(this IReadOnlyList<bool> _this)
		{
			StringBuilder sb = new StringBuilder(_this.Count * 2);
			foreach (bool value in _this)
			{
				byte bvalue = unchecked((byte)(value ? 1 : 0));
				sb.AppendHex(bvalue);
			}

			return new YamlScalarNode(sb.ToString(), true);
		}

		public static YamlNode ExportYaml(this IReadOnlyList<char> _this)
		{
			StringBuilder sb = new StringBuilder(_this.Count * 4);
			foreach (char value in _this)
			{
				sb.AppendHex(value);
			}

			return new YamlScalarNode(sb.ToString(), true);
		}

		public static YamlNode ExportYaml(this IReadOnlyList<byte> _this)
		{
			StringBuilder sb = new StringBuilder(_this.Count * 2);
			foreach (byte value in _this)
			{
				sb.AppendHex(value);
			}

			return new YamlScalarNode(sb.ToString(), true);
		}

		public static YamlNode ExportYaml(this IReadOnlyList<ushort> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 4);
				foreach (ushort value in _this)
				{
					sb.AppendHex(value);
				}

				return new YamlScalarNode(sb.ToString(), true);
			}
			else
			{
				YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
				foreach (ushort value in _this)
				{
					node.Add(value);
				}

				return node;
			}
		}

		public static YamlNode ExportYaml(this IReadOnlyList<short> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 4);
				foreach (short value in _this)
				{
					sb.AppendHex(value);
				}

				return new YamlScalarNode(sb.ToString(), true);
			}
			else
			{
				YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
				foreach (short value in _this)
				{
					node.Add(value);
				}

				return node;
			}
		}

		public static YamlNode ExportYaml(this IReadOnlyList<uint> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 8);
				foreach (uint value in _this)
				{
					sb.AppendHex(value);
				}

				return new YamlScalarNode(sb.ToString(), true);
			}
			else
			{
				YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
				foreach (uint value in _this)
				{
					node.Add(value);
				}

				return node;
			}
		}

		public static YamlNode ExportYaml(this IReadOnlyList<IReadOnlyList<uint>> _this, bool isRaw)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
			foreach (IReadOnlyList<uint> value in _this)
			{
				node.Add(value.ExportYaml(isRaw));
			}

			return node;
		}

		public static YamlNode ExportYaml(this IReadOnlyList<int> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 8);
				foreach (int value in _this)
				{
					sb.AppendHex(value);
				}

				return new YamlScalarNode(sb.ToString(), true);
			}
			else
			{
				YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
				foreach (int value in _this)
				{
					node.Add(value);
				}

				return node;
			}
		}

		public static YamlNode ExportYaml(this IReadOnlyList<ulong> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 16);
				foreach (ulong value in _this)
				{
					sb.AppendHex(value);
				}

				return new YamlScalarNode(sb.ToString(), true);
			}
			else
			{
				YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
				foreach (ulong value in _this)
				{
					node.Add(value);
				}

				return node;
			}
		}

		public static YamlNode ExportYaml(this IReadOnlyList<long> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 16);
				foreach (long value in _this)
				{
					sb.AppendHex(value);
				}

				return new YamlScalarNode(sb.ToString(), true);
			}
			else
			{
				YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
				foreach (long value in _this)
				{
					node.Add(value);
				}

				return node;
			}
		}
	}
}
