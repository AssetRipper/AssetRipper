namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	private sealed class BooleanNode(bool value) : YamlScalarNode
	{
		private protected override void EmitCore(Emitter emitter)
		{
			emitter.Write(value ? 1 : 0);
		}

		public override string Value => value ? "true" : "false";
	}
}
