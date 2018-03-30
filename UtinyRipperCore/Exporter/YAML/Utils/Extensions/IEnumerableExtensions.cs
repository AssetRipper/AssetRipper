using System;
using System.Collections.Generic;
using System.Text;

namespace UtinyRipper.Exporter.YAML
{
	public static class IEnumerableExtensions
	{
		public static YAMLNode ExportYAML(this IEnumerable<bool> _this)
		{
			throw new NotImplementedException();
			/*YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (bool value in _this)
			{
				node.Add(value);
			}
			return node;*/
		}

		public static YAMLNode ExportYAML(this IEnumerable<byte> _this)
		{
			foreach (byte value in _this)
			{
				s_sb.Append(value.ToString("x2"));
			}
			YAMLScalarNode node = new YAMLScalarNode(s_sb.ToString());
			s_sb.Length = 0;
			return node;
		}

		public static YAMLNode ExportYAML(this IEnumerable<ushort> _this)
		{
			throw new NotImplementedException();
			/*YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (ushort value in _this)
			{
				node.Add(value);
			}
			return node;*/
		}

		public static YAMLNode ExportYAML(this IEnumerable<short> _this)
		{
			throw new NotImplementedException();
			/*YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (short value in _this)
			{
				node.Add(value);
			}
			return node;*/
		}

		public static YAMLNode ExportYAML(this IEnumerable<uint> _this, bool isRaw)
		{
			if (isRaw)
			{
				foreach (uint value in _this)
				{
					s_sb.Append(value.ToString("x8"));
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
					s_sb.Append(value.ToString("x8"));
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

		public static YAMLNode ExportYAML(this IEnumerable<ulong> _this)
		{
			throw new NotImplementedException();
			/*YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (ulong value in _this)
			{
				node.Add(value);
			}
			return node;*/
		}

		public static YAMLNode ExportYAML(this IEnumerable<long> _this)
		{
			throw new NotImplementedException();
			/*YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (long value in _this)
			{
				node.Add(value);
			}
			return node;*/
		}

		public static YAMLNode ExportYAML(this IEnumerable<float> _this)
		{
#warning TODO: check
			//throw new NotImplementedException();
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (float value in _this)
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
