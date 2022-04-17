namespace AssetRipper.Yaml
{
	public sealed class YamlDocument
	{
		public YamlDocument() { }

		public YamlScalarNode CreateScalarRoot()
		{
			YamlScalarNode root = new YamlScalarNode();
			Root = root;
			return root;
		}

		public YamlSequenceNode CreateSequenceRoot()
		{
			YamlSequenceNode root = new YamlSequenceNode();
			Root = root;
			return root;
		}

		public YamlMappingNode CreateMappingRoot()
		{
			YamlMappingNode root = new YamlMappingNode();
			Root = root;
			return root;
		}

		internal void Emit(Emitter emitter, bool isSeparator)
		{
			if (isSeparator)
				emitter.Write("---").WriteWhitespace();

			Root.Emit(emitter);
		}

		public YamlNode Root { get; private set; }
	}
}
