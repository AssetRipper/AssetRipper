using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.Assets.Generics;

namespace AssetRipper.AssemblyDumper.AST;

internal sealed class DictionaryNode : SingleNode<PairNode>
{
	private static readonly Lazy<IMethodDefOrRef> defaultConstructor = new(() => SharedState.Instance.Importer.ImportDefaultConstructor(typeof(AssetDictionary<,>)));
	private static readonly Lazy<IMethodDefOrRef> getCount = new(() => ImportMethod(typeof(AssetDictionary<,>), "get_" + nameof(AssetDictionary<object, object>.Count)));
	private static readonly Lazy<IMethodDefOrRef> setCapacity = new(() => ImportMethod(typeof(AssetDictionary<,>), "set_" + nameof(AssetDictionary<object, object>.Capacity)));
	private static readonly Lazy<IMethodDefOrRef> clear = new(() => ImportMethod(typeof(AssetDictionary<,>), nameof(AssetDictionary<object, object>.Clear)));
	private static readonly Lazy<IMethodDefOrRef> addNew = new(() => ImportMethod(typeof(AssetDictionary<,>), nameof(AssetDictionary<object, object>.AddNew)));
	private static readonly Lazy<IMethodDefOrRef> getPair = new(() => ImportMethod(typeof(AssetDictionary<,>), nameof(AssetDictionary<object, object>.GetPair)));
	public DictionaryNode(GenericInstanceTypeSignature typeSignature, Node? parent = null) : base(parent)
	{
		TypeSignature = typeSignature;
		GenericInstanceTypeSignature pairType = SharedState.Instance.Importer.ImportType(typeof(AssetPair<,>)).MakeGenericInstanceType(
			typeSignature.TypeArguments[0],
			typeSignature.TypeArguments[1]);
		Child = new PairNode(pairType, this);
	}

	public override GenericInstanceTypeSignature TypeSignature { get; }

	public TypeSignature KeyTypeSignature => Child.Key.TypeSignature;

	public TypeSignature ValueTypeSignature => Child.Value.TypeSignature;

	/// <summary>
	/// <see cref="AssetDictionary{TKey, TValue}"/> does not implement <see cref="IEquatable{T}"/>.
	/// </summary>
	public override bool Equatable => false;

	public IMethodDefOrRef DefaultConstructor => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, defaultConstructor.Value);
	public IMethodDefOrRef GetCount => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, getCount.Value);
	public IMethodDefOrRef SetCapacity => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, setCapacity.Value);
	public IMethodDefOrRef Clear => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, clear.Value);
	public IMethodDefOrRef AddNew => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, addNew.Value);
	public IMethodDefOrRef GetPair => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, getPair.Value);
}
