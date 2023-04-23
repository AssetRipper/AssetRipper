using System.Text;

namespace AssetRipper.Yaml.Extensions
{
	public static class YamlEnumerableExtensions
	{
		public static YamlNode ExportYaml(this IEnumerable<bool> _this)
		{
			StringBuilder sb = new StringBuilder();
			foreach (bool value in _this)
			{
				byte bvalue = unchecked((byte)(value ? 1 : 0));
				sb.AppendHex(bvalue);
			}
			return new YamlScalarNode(sb.ToString(), true);
		}

		public static YamlNode ExportYaml(this IEnumerable<char> _this)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char value in _this)
			{
				sb.AppendHex(value);
			}

			return new YamlScalarNode(sb.ToString(), true);
		}

		public static YamlNode ExportYaml(this IEnumerable<byte> _this)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte value in _this)
			{
				sb.AppendHex(value);
			}

			return new YamlScalarNode(sb.ToString(), true);
		}

		public static YamlNode ExportYaml(this IEnumerable<ushort> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
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

		public static YamlNode ExportYaml(this IEnumerable<short> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
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

		public static YamlNode ExportYaml(this IEnumerable<uint> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
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

		public static YamlNode ExportYaml(this IEnumerable<int> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
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

		public static YamlNode ExportYaml(this IEnumerable<ulong> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
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

		public static YamlNode ExportYaml(this IEnumerable<long> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
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

		public static YamlNode ExportYaml(this IEnumerable<float> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
			foreach (float value in _this)
			{
				node.Add(value);
			}

			return node;
		}

		public static YamlNode ExportYaml(this IEnumerable<double> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
			foreach (double value in _this)
			{
				node.Add(value);
			}

			return node;
		}

		public static YamlNode ExportYaml(this IEnumerable<string> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
			foreach (string value in _this)
			{
				node.Add(value);
			}

			return node;
		}

		public static YamlNode ExportYaml(this IEnumerable<IEnumerable<string>> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
			foreach (IEnumerable<string> export in _this)
			{
				node.Add(export.ExportYaml());
			}

			return node;
		}
	}
}
