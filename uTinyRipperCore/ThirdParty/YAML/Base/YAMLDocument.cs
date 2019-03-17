using System;

namespace uTinyRipper.YAML
{
	public sealed class YAMLDocument
	{
		internal YAMLDocument()
		{
		}

		public YAMLScalarNode CreateScalarRoot()
		{
			YAMLScalarNode root = new YAMLScalarNode();
			Root = root;
			return root;
		}

		public YAMLSequenceNode CreateSequenceRoot()
		{
			YAMLSequenceNode root = new YAMLSequenceNode();
			Root = root;
			return root;
		}

		public YAMLMappingNode CreateMappingRoot()
		{
			YAMLMappingNode root = new YAMLMappingNode();
			Root = root;
			return root;
		}
		
		internal void Emit(Emitter emitter, bool isSeparator)
		{
			if(isSeparator)
			{
				emitter.Write("---").WriteWhitespace();
			}

			Root.Emit(emitter);
		}

		public YAMLNode Root { get; private set; }
	}
}
