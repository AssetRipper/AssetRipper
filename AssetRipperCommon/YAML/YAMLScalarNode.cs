//#define USE_HEX_FLOAT

using AssetRipper.Core.Extensions;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AssetRipper.Core.YAML
{
	public sealed class YAMLScalarNode : YAMLNode
	{
		public YAMLScalarNode() { }

		public YAMLScalarNode(bool value) : this(value, false) { }

		public YAMLScalarNode(bool value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(byte value) : this(value, false) { }

		public YAMLScalarNode(byte value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(sbyte value) : this(value, false) { }

		public YAMLScalarNode(sbyte value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(short value) : this(value, false) { }

		public YAMLScalarNode(short value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(ushort value) : this(value, false) { }

		public YAMLScalarNode(ushort value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(int value) : this(value, false) { }

		public YAMLScalarNode(int value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(uint value) : this(value, false) { }

		public YAMLScalarNode(uint value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(long value) : this(value, false) { }

		public YAMLScalarNode(long value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(ulong value) : this(value, false) { }

		public YAMLScalarNode(ulong value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(float value) : this(value, false) { }

		public YAMLScalarNode(float value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(double value) : this(value, false) { }

		public YAMLScalarNode(double value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YAMLScalarNode(string value)
		{
			SetValue(value);
			Style = GetStringStyle(value);
		}

		internal YAMLScalarNode(string value, bool _)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public void SetValue(bool value)
		{
			m_value = value ? 1u : 0u;
			m_objectType = ScalarType.Boolean;
		}

		public void SetValue(byte value)
		{
			m_value = value;
			m_objectType = ScalarType.Byte;
		}

		public void SetValue(short value)
		{
			m_value = unchecked((ushort)value);
			m_objectType = ScalarType.Int16;
		}

		public void SetValue(ushort value)
		{
			m_value = value;
			m_objectType = ScalarType.UInt16;
		}

		public void SetValue(int value)
		{
			m_value = unchecked((uint)value);
			m_objectType = ScalarType.Int32;
		}

		public void SetValue(uint value)
		{
			m_value = value;
			m_objectType = ScalarType.UInt32;
		}

		public void SetValue(long value)
		{
			m_value = unchecked((ulong)value);
			m_objectType = ScalarType.Int64;
		}

		public void SetValue(ulong value)
		{
			m_value = value;
			m_objectType = ScalarType.UInt64;
		}

		public void SetValue(float value)
		{
#if USE_HEX_FLOAT
			// It is more precise technic but output looks vague and less readable
			uint hex = BitConverter.SingleToUInt32Bits(value);
			m_string = $"0x{hex.ToHexString()}({value.ToString(CultureInfo.InvariantCulture)})";
			m_objectType = ScalarType.String;
#else
			m_value = BitConverter.SingleToUInt32Bits(value);
			m_objectType = ScalarType.Single;
#endif
		}

		public void SetValue(double value)
		{
#if USE_HEX_FLOAT
			// It is more precise technic but output looks vague and less readable
			ulong hex = BitConverter.DoubleToUInt64Bits(value);
			m_string = $"0x{hex.ToHexString()}({value.ToString(CultureInfo.InvariantCulture)})";
			m_objectType = ScalarType.String;
#else
			m_value = BitConverter.DoubleToUInt64Bits(value);
			m_objectType = ScalarType.Double;
#endif
		}

		public void SetValue(string value)
		{
			m_string = value;
			m_objectType = ScalarType.String;
		}

		internal Emitter ToString(Emitter emitter)
		{
			if (Style == ScalarStyle.Hex)
			{
				return m_objectType switch
				{
					ScalarType.Byte => emitter.WriteHex((byte)m_value),
					ScalarType.SByte => emitter.WriteHex(unchecked((sbyte)m_value)),
					ScalarType.Int16 => emitter.WriteHex(unchecked((short)m_value)),
					ScalarType.UInt16 => emitter.WriteHex((ushort)m_value),
					ScalarType.Int32 => emitter.WriteHex(unchecked((int)m_value)),
					ScalarType.UInt32 => emitter.WriteHex((uint)m_value),
					ScalarType.Int64 => emitter.WriteHex(unchecked((long)m_value)),
					ScalarType.UInt64 => emitter.WriteHex(m_value),
					ScalarType.Single => emitter.WriteHex((uint)m_value),
					ScalarType.Double => emitter.WriteHex(m_value),
					_ => throw new NotImplementedException(m_objectType.ToString()),
				};
			}

			return m_objectType switch
			{
				ScalarType.Boolean => emitter.Write(m_value),
				ScalarType.Byte => emitter.Write(m_value),
				ScalarType.SByte => emitter.Write(unchecked((sbyte)m_value)),
				ScalarType.Int16 => emitter.Write(unchecked((short)m_value)),
				ScalarType.UInt16 => emitter.Write(m_value),
				ScalarType.Int32 => emitter.Write(unchecked((int)m_value)),
				ScalarType.UInt32 => emitter.Write(m_value),
				ScalarType.Int64 => emitter.Write(unchecked((long)m_value)),
				ScalarType.UInt64 => emitter.Write(m_value),
				ScalarType.Single => emitter.Write(BitConverter.UInt32BitsToSingle((uint)m_value)),
				ScalarType.Double => emitter.Write(BitConverter.UInt64BitsToDouble(m_value)),
				ScalarType.String => WriteString(emitter),
				_ => throw new NotImplementedException(m_objectType.ToString()),
			};
		}

		internal override void Emit(Emitter emitter)
		{
			base.Emit(emitter);

			switch (Style)
			{
				case ScalarStyle.Hex:
				case ScalarStyle.Plain:
					ToString(emitter);
					break;

				case ScalarStyle.SingleQuoted:
					emitter.Write('\'');
					ToString(emitter);
					emitter.Write('\'');
					break;

				case ScalarStyle.DoubleQuoted:
					emitter.Write('"');
					ToString(emitter);
					emitter.Write('"');
					break;

				default:
					throw new Exception($"Unsupported scalar style {Style}");
			}
		}

		private Emitter WriteString(Emitter emitter)
		{
			if (Style == ScalarStyle.Plain)
			{
				if (emitter.IsFormatKeys && emitter.IsKey)
				{
					emitter.WriteFormat(m_string);
				}
				else
				{
					emitter.Write(m_string);
				}
			}
			else if (Style == ScalarStyle.SingleQuoted)
			{
				emitter.WriteDelayed();
				for (int i = 0; i < m_string.Length; i++)
				{
					char c = m_string[i];
					emitter.WriteRaw(c);
					if (c == '\'')
					{
						emitter.WriteRaw(c);
					}
					else if (c == '\n')
					{
						emitter.WriteRaw("\n	");
					}
				}
			}
			else if (Style == ScalarStyle.DoubleQuoted)
			{
				emitter.WriteDelayed();
				for (int i = 0; i < m_string.Length; i++)
				{
					char c = m_string[i];
					switch (c)
					{
						case '\\':
							emitter.WriteRaw('\\').WriteRaw('\\');
							break;
						case '\n':
							emitter.WriteRaw('\\').WriteRaw('n');
							break;
						case '\r':
							emitter.WriteRaw('\\').WriteRaw('r');
							break;
						case '\t':
							emitter.WriteRaw('\\').WriteRaw('t');
							break;
						case '"':
							emitter.WriteRaw('\\').WriteRaw('"');
							break;

						default:
							emitter.WriteRaw(c);
							break;
					}
				}
			}
			else
			{
				throw new NotSupportedException(Style.ToString());
			}
			return emitter;
		}

		private static ScalarStyle GetStringStyle(string value)
		{
			if (!string.IsNullOrEmpty(value) && s_illegal.IsMatch(value))
			{
				return value.Contains("\n ") ? ScalarStyle.DoubleQuoted : ScalarStyle.SingleQuoted;
			}
			return ScalarStyle.Plain;
		}

		public static YAMLScalarNode Empty { get; } = new YAMLScalarNode();

		public override YAMLNodeType NodeType => YAMLNodeType.Scalar;
		public override bool IsMultiline => false;
		public override bool IsIndent => false;

		public string Value
		{
			get
			{
				if (Style == ScalarStyle.Hex)
				{
					return m_objectType switch
					{
						ScalarType.Byte => unchecked((byte)m_value).ToHexString(),
						ScalarType.SByte => unchecked((sbyte)m_value).ToHexString(),
						ScalarType.Int16 => unchecked((short)m_value).ToHexString(),
						ScalarType.UInt16 => unchecked((ushort)m_value).ToHexString(),
						ScalarType.Int32 => unchecked((int)m_value).ToHexString(),
						ScalarType.UInt32 => unchecked((uint)m_value).ToHexString(),
						ScalarType.Int64 => unchecked((long)m_value).ToHexString(),
						ScalarType.UInt64 => m_value.ToHexString(),
						ScalarType.Single => BitConverter.UInt32BitsToSingle((uint)m_value).ToHexString(),
						ScalarType.Double => BitConverter.UInt64BitsToDouble(m_value).ToHexString(),
						_ => throw new NotImplementedException(m_objectType.ToString()),
					};
				}

				return m_objectType switch
				{
					ScalarType.Boolean => m_value == 1 ? "true" : "false",
					ScalarType.Byte => m_value.ToString(),
					ScalarType.SByte => unchecked((sbyte)m_value).ToString(),
					ScalarType.Int16 => unchecked((short)m_value).ToString(),
					ScalarType.UInt16 => m_value.ToString(),
					ScalarType.Int32 => unchecked((int)m_value).ToString(),
					ScalarType.UInt32 => m_value.ToString(),
					ScalarType.Int64 => unchecked((long)m_value).ToString(),
					ScalarType.UInt64 => m_value.ToString(),
					ScalarType.Single => BitConverter.UInt32BitsToSingle((uint)m_value).ToString(CultureInfo.InvariantCulture),
					ScalarType.Double => BitConverter.UInt64BitsToDouble(m_value).ToString(CultureInfo.InvariantCulture),
					ScalarType.String => m_string,
					_ => throw new NotImplementedException(m_objectType.ToString()),
				};
			}
			set => m_string = value;
		}
		public ScalarStyle Style { get; }

		private static readonly Regex s_illegal = new("(^\\s)|(^-\\s)|(^-$)|(^[\\:\\[\\]'\"*&!@#%{}?<>,\\`])|([:@]\\s)|([\\n\\r])|([:\\s]$)", RegexOptions.Compiled);

		private ScalarType m_objectType = ScalarType.String;
		private string m_string = string.Empty;
		private ulong m_value = 0;
	}
}
