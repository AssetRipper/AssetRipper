//#define USE_HEX_FLOAT
using AssetRipper.Primitives;
using AssetRipper.Yaml.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AssetRipper.Yaml
{
	public partial class YamlScalarNode : YamlNode
	{
		public static YamlScalarNode Create(bool value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(bool value, bool isHex = false)
		{
			m_value = value ? 1u : 0u;
			m_objectType = ScalarType.Boolean;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(byte value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(byte value, bool isHex = false)
		{
			m_value = value;
			m_objectType = ScalarType.Byte;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(sbyte value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(sbyte value, bool isHex = false)
		{
			m_value = unchecked((byte)value);
			m_objectType = ScalarType.SByte;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(short value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(short value, bool isHex = false)
		{
			m_value = unchecked((ushort)value);
			m_objectType = ScalarType.Int16;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(ushort value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(ushort value, bool isHex = false)
		{
			m_value = value;
			m_objectType = ScalarType.UInt16;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(int value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(int value, bool isHex = false)
		{
			m_value = unchecked((uint)value);
			m_objectType = ScalarType.Int32;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(uint value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(uint value, bool isHex = false)
		{
			m_value = value;
			m_objectType = ScalarType.UInt32;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(long value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(long value, bool isHex = false)
		{
			m_value = unchecked((ulong)value);
			m_objectType = ScalarType.Int64;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(ulong value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(ulong value, bool isHex = false)
		{
			m_value = value;
			m_objectType = ScalarType.UInt64;
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(float value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(float value, bool isHex = false)
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
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(double value, bool isHex = false) => new(value, isHex);

		private YamlScalarNode(double value, bool isHex = false)
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
			Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(string value) => new(value);

		private YamlScalarNode(string value)
		{
			m_string = value;
			m_objectType = ScalarType.String;
			Style = GetStringStyle(value);
		}

		internal static YamlScalarNode CreatePlain(string value) => new(value, true);

		private YamlScalarNode(string value, bool _)
		{
			m_string = value;
			m_objectType = ScalarType.String;
			Style = ScalarStyle.Plain;
		}

		public static YamlScalarNode Create(Utf8String value) => new(value);

		private YamlScalarNode(Utf8String value) : this(value.String)
		{
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
		}
		public ScalarStyle Style { get; }

		private readonly ScalarType m_objectType = ScalarType.String;
		private readonly string m_string = string.Empty;
		private readonly ulong m_value = 0;

		public override string ToString() => Value;

		[GeneratedRegex("""(^\s)|(^-\s)|(^-$)|(^[\:\[\]'"*&!@#%{}?<>,\`])|([:@]\s)|([\n\r])|([:\s]$)""", RegexOptions.Compiled)]
		private static partial Regex IllegalStringsRegex();
	}
}
