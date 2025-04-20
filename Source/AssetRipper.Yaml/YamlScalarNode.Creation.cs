using AssetRipper.Primitives;
using System.Numerics;

namespace AssetRipper.Yaml;

public abstract partial class YamlScalarNode
{
	public static YamlScalarNode Create<T>(T value) where T : unmanaged, INumber<T>, IConvertible
	{
		return new NumericNode<T>(value);
	}

	public static YamlScalarNode CreateHex<T>(IReadOnlyList<T> list) where T : unmanaged, INumber<T>
	{
		return new NumericListNode<T>(list);
	}

	public static YamlScalarNode CreateHex(IReadOnlyList<bool> list) => new BoolListNode(list);

	public static YamlScalarNode CreateHex(IReadOnlyList<char> list) => new CharListNode(list);

	public static YamlScalarNode CreateHex<T>(T value) where T : IBinaryFloatingPointIeee754<T>
	{
		return new FloatingPointHexNode<T>(value);
	}

	public static YamlScalarNode Create(bool value) => new BooleanNode(value);

	public static YamlScalarNode Create(string value) => new StringNode(value);

	public static YamlScalarNode Create(Utf8String value) => new StringNode(value.String);

	internal static YamlScalarNode CreatePlain(string value) => new StringNode(value, ScalarStyle.Plain);
}
