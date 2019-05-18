using System.Collections.Generic;
using System.Text;

namespace uTinyRipper.YAML
{
	public static class IEnumerableExtensions
	{
		public static YAMLNode ExportYAML(this IEnumerable<bool> _this)
		{
			StringBuilder sb = new StringBuilder();
			foreach (bool value in _this)
			{
				byte bvalue = unchecked((byte)(value ? 1 : 0));
				sb.AppendHex(bvalue);
			}
			return new YAMLScalarNode(sb.ToString(), true);
		}

		public static YAMLNode ExportYAML(this IEnumerable<char> _this)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char value in _this)
			{
				sb.AppendHex((ushort)value);
			}
			return new YAMLScalarNode(sb.ToString(), true);
		}

		public static YAMLNode ExportYAML(this IEnumerable<byte> _this)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte value in _this)
			{
				sb.AppendHex(value);
			}
			return new YAMLScalarNode(sb.ToString(), true);
		}

		public static YAMLNode ExportYAML(this IEnumerable<ushort> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
				foreach (ushort value in _this)
				{
					sb.AppendHex(value);
				}
				return new YAMLScalarNode(sb.ToString(), true);
			}
			else
			{
				YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
				foreach (ushort value in _this)
				{
					node.Add(value);
				}
				return node;
			}
		}

		public static YAMLNode ExportYAML(this IEnumerable<short> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
				foreach (short value in _this)
				{
					sb.AppendHex(value);
				}
				return new YAMLScalarNode(sb.ToString(), true);
			}
			else
			{
				YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
				foreach (short value in _this)
				{
					node.Add(value);
				}
				return node;
			}
		}

		public static YAMLNode ExportYAML(this IEnumerable<uint> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
				foreach (uint value in _this)
				{
					sb.AppendHex(value);
				}
				return new YAMLScalarNode(sb.ToString(), true);
			}
			else
			{
				YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
				foreach (uint value in _this)
				{
					node.Add(value);
				}
				return node;
			}
		}

		public static YAMLNode ExportYAML(this IEnumerable<int> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
				foreach (int value in _this)
				{
					sb.AppendHex(value);
				}
				return new YAMLScalarNode(sb.ToString(), true);
			}
			else
			{
				YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
				foreach (int value in _this)
				{
					node.Add(value);
				}
				return node;
			}
		}

		public static YAMLNode ExportYAML(this IEnumerable<ulong> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
				foreach (ulong value in _this)
				{
					sb.AppendHex(value);
				}
				return new YAMLScalarNode(sb.ToString(), true);
			}
			else
			{
				YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
				foreach (ulong value in _this)
				{
					node.Add(value);
				}
				return node;
			}
		}

		public static YAMLNode ExportYAML(this IEnumerable<long> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder();
				foreach (long value in _this)
				{
					sb.AppendHex(value);
				}
				return new YAMLScalarNode(sb.ToString(), true);
			}
			else
			{
				YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
				foreach (long value in _this)
				{
					node.Add(value);
				}
				return node;
			}
		}

		public static YAMLNode ExportYAML(this IEnumerable<float> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (float value in _this)
			{
				node.Add(value);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IEnumerable<double> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (double value in _this)
			{
				node.Add(value);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IEnumerable<string> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (string value in _this)
			{
				node.Add(value);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IEnumerable<IEnumerable<string>> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (IEnumerable<string> export in _this)
			{
				node.Add(export.ExportYAML());
			}
			return node;
		}
	}
}
