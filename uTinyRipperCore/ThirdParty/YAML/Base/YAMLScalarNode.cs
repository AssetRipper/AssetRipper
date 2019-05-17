//#define USE_HEX_FLOAT

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace uTinyRipper.YAML
{
	public sealed class YAMLScalarNode : YAMLNode
	{
		public YAMLScalarNode()
		{
		}

		public YAMLScalarNode(bool value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(byte value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(short value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(ushort value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(int value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(uint value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}
		
		public YAMLScalarNode(long value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(ulong value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(float value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(double value)
		{
			SetValue(value);
			Style = ScalarStyle.Plain;
		}

		public YAMLScalarNode(string value)
		{
			SetValue(value);
			UpdateStyle();
		}

		public void SetValue(bool value)
		{
			m_value = value ? 1 : 0;
			m_objectType = ScalarType.Boolean;
		}

		public void SetValue(byte value)
		{
			m_value = value;
			m_objectType = ScalarType.Byte;
		}

		public void SetValue(short value)
		{
			m_value = value;
			m_objectType = ScalarType.Int16;
		}

		public void SetValue(ushort value)
		{
			m_value = value;
			m_objectType = ScalarType.UInt16;
		}

		public void SetValue(int value)
		{
			m_value = value;
			m_objectType = ScalarType.Int32;
		}

		public void SetValue(uint value)
		{
			m_value = value;
			m_objectType = ScalarType.UInt32;
		}

		public void SetValue(long value)
		{
			m_value = value;
			m_objectType = ScalarType.Int64;
		}

		public void SetValue(ulong value)
		{
			m_value = unchecked((long)value);
			m_objectType = ScalarType.UInt64;
		}

		public void SetValue(float value)
		{
#if USE_HEX_FLOAT
			// It is more precise technic but output looks vague and less readable
			uint hex = BitConverterExtensions.ToUInt32(value);
			m_string = $"0x{hex.ToHexString()}({value.ToString(CultureInfo.InvariantCulture)})";
			m_objectType = ScalarType.String;
#else
			m_double = value;
			m_objectType = ScalarType.Single;
#endif
		}

		public void SetValue(double value)
		{
#if USE_HEX_FLOAT
			// It is more precise technic but output looks vague and less readable
			ulong hex = BitConverterExtensions.ToUInt64(value);
			m_string = $"0x{hex.ToHexString()}({value.ToString(CultureInfo.InvariantCulture)})";
			m_objectType = ScalarType.String;
#else
			m_double = value;
			m_objectType = ScalarType.Double;
#endif
		}

		public void SetValue(string value)
		{
			m_string = value;
			m_objectType = ScalarType.String;
		}

		public StringBuilder ToString(StringBuilder sb)
		{
			if (Style == ScalarStyle.Hex)
			{
				switch (m_objectType)
				{
					case ScalarType.Byte:
						return sb.AppendHex((byte)m_value);
					case ScalarType.Int16:
						return sb.AppendHex((short)m_value);
					case ScalarType.UInt16:
						return sb.AppendHex((ushort)m_value);
					case ScalarType.Int32:
						return sb.AppendHex((int)m_value);
					case ScalarType.UInt32:
						return sb.AppendHex((uint)m_value);
					case ScalarType.Int64:
						return sb.AppendHex(m_value);
					case ScalarType.UInt64:
						return sb.AppendHex(unchecked((ulong)m_value));
					case ScalarType.Single:
						return sb.AppendHex((float)m_double);
					case ScalarType.Double:
						return sb.AppendHex(m_double);
					case ScalarType.String:
						return sb.Append(m_string);
					default:
						throw new NotImplementedException(m_objectType.ToString());
				}
			}

			switch (m_objectType)
			{
				case ScalarType.UInt64:
					return sb.Append(unchecked((ulong)m_value));
				case ScalarType.Single:
					return sb.Append(((float)m_double).ToString(CultureInfo.InvariantCulture));
				case ScalarType.Double:
					return sb.Append(m_double.ToString(CultureInfo.InvariantCulture));
				case ScalarType.String:
					return sb.Append(m_string);

				default:
					return sb.Append(m_value);
			}
		}

		internal Emitter ToString(Emitter emitter)
		{
			if (Style == ScalarStyle.Hex)
			{
				switch (m_objectType)
				{
					case ScalarType.Byte:
						return emitter.WriteHex((byte)m_value);
					case ScalarType.Int16:
						return emitter.WriteHex((short)m_value);
					case ScalarType.UInt16:
						return emitter.WriteHex((ushort)m_value);
					case ScalarType.Int32:
						return emitter.WriteHex((int)m_value);
					case ScalarType.UInt32:
						return emitter.WriteHex((uint)m_value);
					case ScalarType.Int64:
						return emitter.WriteHex(m_value);
					case ScalarType.UInt64:
						return emitter.WriteHex(unchecked((ulong)m_value));
					case ScalarType.Single:
						return emitter.WriteHex((float)m_double);
					case ScalarType.Double:
						return emitter.WriteHex(m_double);
					case ScalarType.String:
						return emitter.Write(m_string);
					default:
						throw new NotImplementedException(m_objectType.ToString());
				}
			}

			switch (m_objectType)
			{
				case ScalarType.UInt64:
					return emitter.Write(unchecked((ulong)m_value));
				case ScalarType.Single:
					return emitter.Write((float)m_double);
				case ScalarType.Double:
					return emitter.Write(m_double);
				case ScalarType.String:
					return emitter.Write(m_string);

				default:
					return emitter.Write(m_value);
			}
		}

		internal override void Emit(Emitter emitter)
		{
			base.Emit(emitter);

			switch(Style)
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

		private void UpdateStyle()
		{
			string value = m_string;
			if (s_illegal.IsMatch(value))
			{
				if (value.Contains('\''))
				{
					if (value.Contains('"'))
					{
						value = value.Replace("'", "''");
						m_string = value;
						Style = ScalarStyle.SingleQuoted;
					}
					else
					{
						Style = ScalarStyle.DoubleQuoted;
					}
				}
				else
				{
					Style = ScalarStyle.SingleQuoted;
				}
			}
			else
			{
				Style = ScalarStyle.Plain;
			}
		}

		public static YAMLScalarNode Empty { get; } = new YAMLScalarNode();

		public override YAMLNodeType NodeType => YAMLNodeType.Scalar;
		public override bool IsMultyline => false;
		public override bool IsIndent => false;

		public string Value
		{
			get
			{
				if (Style == ScalarStyle.Hex)
				{
					switch (m_objectType)
					{
						case ScalarType.Byte:
							return ((byte)m_value).ToHexString();
						case ScalarType.Int16:
							return ((short)m_value).ToHexString();
						case ScalarType.UInt16:
							return ((ushort)m_value).ToHexString();
						case ScalarType.Int32:
							return ((int)m_value).ToHexString();
						case ScalarType.UInt32:
							return ((uint)m_value).ToHexString();
						case ScalarType.Int64:
							return m_value.ToHexString();
						case ScalarType.UInt64:
							return (unchecked((ulong)m_value)).ToHexString();
						case ScalarType.Single:
							return ((float)m_double).ToHexString();
						case ScalarType.Double:
							return m_double.ToHexString();
						case ScalarType.String:
							return m_string;
						default:
							throw new NotImplementedException(m_objectType.ToString());
					}
				}

				switch (m_objectType)
				{
					case ScalarType.Boolean:
						return m_value == 0 ? 0.ToString() : 1.ToString();
					case ScalarType.Single:
						return ((float)m_double).ToString(CultureInfo.InvariantCulture);
					case ScalarType.Double:
						return m_double.ToString(CultureInfo.InvariantCulture);
					case ScalarType.String:
						return m_string;

					default:
						return m_value.ToString();
				}
			}
			set => m_string = value;
		}
		public ScalarStyle Style { get; set; }

		private static readonly Regex s_illegal = new Regex("(^\\s)|(^-\\s)|(^-$)|(^[\\:\\[\\]'\"*&!@#%{}?<>,\\`])|([:@]\\s)|([\\n\\r])|([:\\s]$)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private ScalarType m_objectType = ScalarType.String;
		private string m_string = string.Empty;
		private long m_value = 0;
		private double m_double = 0.0;
	}
}
