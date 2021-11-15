namespace AssetRipper.Core.YAML
{
	public enum YAMLNodeType
	{
		/// <summary>
		/// The node is a <see cref="YAMLMappingNode"/>.
		/// </summary>
		Mapping,

		/// <summary>
		/// The node is a <see cref="YAMLScalarNode"/>.
		/// </summary>
		Scalar,

		/// <summary>
		/// The node is a <see cref="YAMLSequenceNode"/>.
		/// </summary>
		Sequence
	}
}
