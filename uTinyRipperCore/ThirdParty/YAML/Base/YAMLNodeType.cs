namespace uTinyRipper.YAML
{
	public enum YAMLNodeType
	{
		/// <summary>
		/// The node is a <see cref="YamlMappingNode"/>.
		/// </summary>
		Mapping,

		/// <summary>
		/// The node is a <see cref="YamlScalarNode"/>.
		/// </summary>
		Scalar,

		/// <summary>
		/// The node is a <see cref="YamlSequenceNode"/>.
		/// </summary>
		Sequence
	}
}
