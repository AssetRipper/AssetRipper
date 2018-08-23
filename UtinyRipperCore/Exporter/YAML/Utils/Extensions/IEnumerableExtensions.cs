using System;
using System.Collections.Generic;
using System.Text;

namespace UtinyRipper.Exporter.YAML
{
	public static class IEnumerableExtensions
	{
		public static YAMLNode ExportYAML(this IEnumerable<bool> _this)
		{
			foreach (bool value in _this)
			{
				byte bvalue = unchecked((byte)(value ? 1 : 0));
				s_sb.Append(bvalue.ToHexString());
			}
			YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
			s_sb.Length = 0;
			return node;
		}

		public static YAMLNode ExportYAML(this IEnumerable<char> _this)
		{
			foreach (char value in _this)
			{
				s_sb.Append(((ushort)value).ToHexString());
			}
			YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
			s_sb.Length = 0;
			return node;
		}

		public static YAMLNode ExportYAML(this IEnumerable<byte> _this)
		{
			foreach (byte value in _this)
			{
				s_sb.Append(value.ToHexString());
			}
			YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
			s_sb.Length = 0;
			return node;
		}

		public static YAMLNode ExportYAML(this IEnumerable<ushort> _this, bool isRaw)
		{
			if (isRaw)
			{
				foreach (ushort value in _this)
				{
					s_sb.Append(value.ToHexString());
				}
				YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
				s_sb.Length = 0;
				return node;
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
				foreach (short value in _this)
				{
					s_sb.Append(value.ToHexString());
				}
				YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
				s_sb.Length = 0;
				return node;
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
				foreach (uint value in _this)
				{
					s_sb.Append(value.ToHexString());
				}
				YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
				s_sb.Length = 0;
				return node;
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
				foreach (int value in _this)
				{
					s_sb.Append(value.ToHexString());
				}
				YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
				s_sb.Length = 0;
				return node;
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
				foreach (ulong value in _this)
				{
					s_sb.Append(value.ToHexString());
				}
				YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
				s_sb.Length = 0;
				return node;
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
				foreach (long value in _this)
				{
					s_sb.Append(value.ToHexString());
				}
				YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
				s_sb.Length = 0;
				return node;
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

		private static readonly StringBuilder s_sb = new StringBuilder();
	}
}
