using System.Collections.Generic;
using System.Text;

namespace uTinyRipper.YAML
{
	public static class IReadOnlyListExtensions
	{
		public static YAMLNode ExportYAML(this IReadOnlyList<bool> _this)
		{
			StringBuilder sb = new StringBuilder(_this.Count * 2);
			foreach (bool value in _this)
			{
				byte bvalue = unchecked((byte)(value ? 1 : 0));
				sb.AppendHex(bvalue);
			}
			return new YAMLScalarNode(sb.ToString(), true);
		}

		public static YAMLNode ExportYAML(this IReadOnlyList<char> _this)
		{
			StringBuilder sb = new StringBuilder(_this.Count * 4);
			foreach (char value in _this)
			{
				sb.AppendHex((ushort)value);
			}
			return new YAMLScalarNode(sb.ToString(), true);
		}

		public static YAMLNode ExportYAML(this IReadOnlyList<byte> _this)
		{
			StringBuilder sb = new StringBuilder(_this.Count * 2);
			foreach (byte value in _this)
			{
				sb.AppendHex(value);
			}
			return new YAMLScalarNode(sb.ToString(), true);
		}

		public static YAMLNode ExportYAML(this IReadOnlyList<ushort> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 4);
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

		public static YAMLNode ExportYAML(this IReadOnlyList<short> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 4);
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

		public static YAMLNode ExportYAML(this IReadOnlyList<uint> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 8);
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

		public static YAMLNode ExportYAML(this IReadOnlyList<int> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 8);
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

		public static YAMLNode ExportYAML(this IReadOnlyList<ulong> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 16);
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

		public static YAMLNode ExportYAML(this IReadOnlyList<long> _this, bool isRaw)
		{
			if (isRaw)
			{
				StringBuilder sb = new StringBuilder(_this.Count * 16);
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
	}
}
