using System.Text.RegularExpressions;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	private sealed partial class StringNode : YamlScalarNode
	{
		public StringNode(string value)
		{
			Value = value;
			Style = GetStringStyle(value);
		}

		public StringNode(string value, ScalarStyle style)
		{
			Value = value;
			Style = style;
		}

		private protected override void EmitCore(Emitter emitter)
		{
			WriteString(emitter);
		}

		public override string Value { get; }
		public override ScalarStyle Style { get; }

		private void WriteString(Emitter emitter)
		{
			switch (Style)
			{
				case ScalarStyle.Plain:
					if (emitter.IsFormatKeys && emitter.IsKey)
					{
						emitter.WriteFormat(Value);
					}
					else
					{
						emitter.Write(Value);
					}
					break;
				case ScalarStyle.SingleQuoted:
					{
						emitter.WriteDelayed();
						for (int i = 0; i < Value.Length; i++)
						{
							char c = Value[i];
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
						for (int i = 0; i < Value.Length; i++)
						{
							char c = Value[i];
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
		}

		private static ScalarStyle GetStringStyle(string value)
		{
			if (value == null)
			{
				return ScalarStyle.Plain;
			}
			if (value.Length == 0 || MatchOtherTypesRegex().IsMatch(value))
			{
				return ScalarStyle.SingleQuoted;
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

		[GeneratedRegex("""(^\s)|(^-\s)|(^-$)|(^[\:\[\]'"*&!@#%{}?<>,\`])|([:@]\s)|(\s#)|([\n\r])|([:\s]$)""", RegexOptions.Compiled)]
		private static partial Regex IllegalStringsRegex();

		[GeneratedRegex("""^(?:~|[nN]ull|NULL|[yY](?:es)?|YES|[nN]o|NO|[tT]rue|TRUE|[fF]alse|FALSE|[oO](?:n|ff)|ON|OFF|[-+]?0b[0-1_]+|[-+]?0[0-7_]+|[-+]?(?:0|[1-9][0-9_]*)|[-+]?0x[0-9a-fA-F_]+|[-+]?[1-9][0-9_]*(?::[0-5]?[0-9])+|[-+]?(?:[0-9][0-9_]*)?\.[0-9_]*(?:[eE][-+][0-9]+)?|[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+\.[0-9_]*|[-+]?\.(?:inf|Inf|INF)|\.(?:nan|NaN|NAN))$""", RegexOptions.Compiled)]
		private static partial Regex MatchOtherTypesRegex();
	}
}
