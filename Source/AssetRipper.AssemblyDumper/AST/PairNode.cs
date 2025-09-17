using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.Assets.Generics;

namespace AssetRipper.AssemblyDumper.AST;

internal sealed class PairNode : Node
{
	private static readonly Lazy<IMethodDefOrRef> defaultConstructor = new(() => SharedState.Instance.Importer.ImportDefaultConstructor(typeof(AssetPair<,>)));
	private static readonly Lazy<IMethodDefOrRef> getKey = new(() => ImportMethod(typeof(AssetPair<,>), "get_" + nameof(AssetPair<object, object>.Key)));
	private static readonly Lazy<IMethodDefOrRef> setKey = new(() => ImportMethod(typeof(AssetPair<,>), "set_" + nameof(AssetPair<object, object>.Key)));
	private static readonly Lazy<IMethodDefOrRef> getValue = new(() => ImportMethod(typeof(AssetPair<,>), "get_" + nameof(AssetPair<object, object>.Value)));
	private static readonly Lazy<IMethodDefOrRef> setValue = new(() => ImportMethod(typeof(AssetPair<,>), "set_" + nameof(AssetPair<object, object>.Value)));
	private static readonly Lazy<IMethodDefOrRef> implicitConversion = new(() => ImportMethod(typeof(AssetPair<,>), "op_Implicit"));

	public PairNode(GenericInstanceTypeSignature typeSignature, Node? parent = null) : base(parent)
	{
		TypeSignature = typeSignature;
		Key = new(typeSignature.TypeArguments[0], this);
		Value = new(typeSignature.TypeArguments[1], this);
	}

	public KeyNode Key { get; }
	public ValueNode Value { get; }

	public override IReadOnlyList<Node> Children => [Key, Value];

	public override bool AnyPPtrs => Key.AnyPPtrs || Value.AnyPPtrs;

	public override bool Equatable => Key.Equatable && Value.Equatable;

	public override GenericInstanceTypeSignature TypeSignature { get; }

	public IMethodDefOrRef DefaultConstructor => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, defaultConstructor.Value);
	public IMethodDefOrRef GetKey => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, getKey.Value);
	public IMethodDefOrRef SetKey => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, setKey.Value);
	public IMethodDefOrRef GetValue => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, getValue.Value);
	public IMethodDefOrRef SetValue => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, setValue.Value);
	public IMethodDefOrRef ImplicitConversion => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, implicitConversion.Value);
}
