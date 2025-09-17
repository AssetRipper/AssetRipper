using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.Assets.Generics;

namespace AssetRipper.AssemblyDumper.AST;

internal sealed class ListNode : SingleNode<Node>
{
	private static readonly Lazy<IMethodDefOrRef> defaultConstructor = new(() => SharedState.Instance.Importer.ImportDefaultConstructor(typeof(AssetList<>)));
	private static readonly Lazy<IMethodDefOrRef> getCount = new(() => ImportMethod(typeof(AssetList<>), "get_" + nameof(AssetList<object>.Count)));
	private static readonly Lazy<IMethodDefOrRef> setCapacity = new(() => ImportMethod(typeof(AssetList<>), "set_" + nameof(AssetList<object>.Capacity)));
	private static readonly Lazy<IMethodDefOrRef> getItem = new(() => ImportMethod(typeof(AssetList<>), "get_Item"));
	private static readonly Lazy<IMethodDefOrRef> clear = new(() => ImportMethod(typeof(AssetList<>), nameof(AssetList<object>.Clear)));
	private static readonly Lazy<IMethodDefOrRef> add = new(() => ImportMethod(typeof(AssetList<>), nameof(AssetList<object>.Add)));
	private static readonly Lazy<IMethodDefOrRef> addNew = new(() => ImportMethod(typeof(AssetList<>), nameof(AssetList<object>.AddNew)));

	public ListNode(GenericInstanceTypeSignature typeSignature, Node? parent = null) : base(parent)
	{
		TypeSignature = typeSignature;
		Child = Create(typeSignature.TypeArguments[0], this);
	}

	public override GenericInstanceTypeSignature TypeSignature { get; }
	public TypeSignature ElementTypeSignature => TypeSignature.TypeArguments[0];
	/// <summary>
	/// <see cref="AssetList{T}"/> does not implement <see cref="IEquatable{T}"/>.
	/// </summary>
	public override bool Equatable => false;

	public IMethodDefOrRef DefaultConstructor => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, defaultConstructor.Value);
	public IMethodDefOrRef GetCount => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, getCount.Value);
	public IMethodDefOrRef SetCapacity => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, setCapacity.Value);
	public IMethodDefOrRef GetItem => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, getItem.Value);
	public IMethodDefOrRef Clear => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, clear.Value);
	public IMethodDefOrRef Add => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, add.Value);
	public IMethodDefOrRef AddNew => MethodUtils.MakeMethodOnGenericType(SharedState.Instance.Importer, TypeSignature, addNew.Value);
}
