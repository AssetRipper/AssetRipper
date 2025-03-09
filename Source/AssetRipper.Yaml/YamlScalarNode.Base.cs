namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode : YamlNode
{
	public sealed override bool IsMultiline => false;
	public sealed override bool IsIndent => false;
	public sealed override YamlNodeType NodeType => YamlNodeType.Scalar;
	public virtual ScalarStyle Style => ScalarStyle.Plain;
	public abstract string Value { get; }

	private YamlScalarNode()
	{
	}

	private protected abstract void EmitCore(Emitter emitter);

	internal sealed override void Emit(Emitter emitter)
	{
		base.Emit(emitter);

		switch (Style)
		{
			case ScalarStyle.Plain:
				EmitCore(emitter);
				break;

			case ScalarStyle.SingleQuoted:
				emitter.Write('\'');
				EmitCore(emitter);
				emitter.Write('\'');
				break;

			case ScalarStyle.DoubleQuoted:
				emitter.Write('"');
				EmitCore(emitter);
				emitter.Write('"');
				break;

			default:
				throw new Exception($"Unsupported scalar style {Style}");
		}
	}


	public sealed override string ToString() => Value;
}
