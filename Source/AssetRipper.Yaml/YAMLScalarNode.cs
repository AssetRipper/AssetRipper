//#define USE_HEX_FLOAT
using AssetRipper.Primitives;
using AssetRipper.Yaml.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AssetRipper.Yaml
{
	public sealed partial class YamlScalarNode : YamlNode
	{
		public YamlScalarNode() { }

		public YamlScalarNode(bool value) : this(value, false) { }

		public YamlScalarNode(bool value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(byte value) : this(value, false) { }

		public YamlScalarNode(byte value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(sbyte value) : this(value, false) { }

		public YamlScalarNode(sbyte value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(short value) : this(value, false) { }

		public YamlScalarNode(short value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(ushort value) : this(value, false) { }

		public YamlScalarNode(ushort value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(int value) : this(value, false) { }

		public YamlScalarNode(int value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(uint value) : this(value, false) { }

		public YamlScalarNode(uint value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(long value) : this(value, false) { }

		public YamlScalarNode(long value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(ulong value) : this(value, false) { }

		public YamlScalarNode(ulong value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(float value) : this(value, false) { }

		public YamlScalarNode(float value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(double value) : this(value, false) { }

		public YamlScalarNode(double value, bool isHex)
		{
			SetValue(value);
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public YamlScalarNode(string value)
		{
			SetValue(value);
			Style = GetStringStyle(value);
		}

		internal YamlScalarNode(string value, bool _)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YamlScalarNode(Utf8String value) : this(value.String)
		{
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
			return Style switch
			{
				ScalarStyle.Hex => m_objectType switch
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
				},
				_ => m_objectType switch
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
				},
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
			switch (Style)
			{
				case ScalarStyle.Plain:
					if (emitter.IsFormatKeys && emitter.IsKey)
					{
						emitter.WriteFormat(m_string);
					}
					else
					{
						emitter.Write(m_string);
					}
					break;
				case ScalarStyle.SingleQuoted:
					{
						emitter.WriteDelayed();
						for (int i = 0; i < m_string.Length; i++)
						{
							char c = m_string[i];
							emitter.WriteRaw(c);
							switch (c)
							{
								case '\'':
									emitter.WriteRaw(c);
									break;
								case '\n':
									emitter.WriteRaw("\n	");
									break;
							}
						}

						break;
					}

				case ScalarStyle.DoubleQuoted:
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
									if (char.IsControl(c))
									{
										emitter.WriteRawUnicode(c);
									}
									else
									{
										emitter.WriteRaw(c);
									}
									break;
							}
						}

						break;
					}

				default:
					throw new NotSupportedException(Style.ToString());
			}

			return emitter;
		}

		private static ScalarStyle GetStringStyle(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return ScalarStyle.Plain;
			}
			else if (ContainsControlCharacter(value))
			{
				return ScalarStyle.DoubleQuoted;
			}
			else if (IllegalStringsRegex().IsMatch(value))
			{
				return value.Contains("\n ") ? ScalarStyle.DoubleQuoted : ScalarStyle.SingleQuoted;
			}

			return ScalarStyle.Plain;
		}

		private static bool ContainsControlCharacter(ReadOnlySpan<char> span)
		{
			foreach (char c in span)
			{
				if (char.IsControl(c))
				{
					return true;
				}
			}
			return false;
		}

		public override YamlNodeType NodeType => YamlNodeType.Scalar;
		public override bool IsMultiline => false;
		public override bool IsIndent => false;

		public string Value
		{
			get
			{
				return Style switch
				{
					ScalarStyle.Hex => m_objectType switch
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
					},
					_ => m_objectType switch
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
					},
				};
			}
			set => m_string = value;
		}
		public ScalarStyle Style { get; }

		private ScalarType m_objectType = ScalarType.String;
		private string m_string = string.Empty;
		private ulong m_value = 0;

		public override string ToString() => Value;

		[GeneratedRegex("(^\\s)|(^-\\s)|(^-$)|(^[\\:\\[\\]'\"*&!@#%{}?<>,\\`])|([:@]\\s)|([\\n\\r])|([:\\s]$)", RegexOptions.Compiled)]
		private static partial Regex IllegalStringsRegex();
	}
}
