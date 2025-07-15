using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Processing.ScriptableObject;

public sealed class ScriptableObjectGroup : AssetGroup, INamed
{
	public ScriptableObjectGroup(AssetInfo assetInfo, IMonoBehaviour root) : base(assetInfo)
	{
		Root = root;
	}

	public IMonoBehaviour Root { get; }
	public List<IMonoBehaviour> Children { get; } = [];

	public override IEnumerable<IMonoBehaviour> Assets => Children.Prepend(Root);

	public Utf8String Name { get => Root.Name; set => throw new NotSupportedException(); }

	public string? FileExtension { get; set; }

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		yield return (nameof(Root), AssetToPPtr(Root));
		foreach (IMonoBehaviour child in Children)
		{
			yield return ($"{nameof(Children)}[]", AssetToPPtr(child));
		}
	}

	public override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			this.WalkPrimitiveField(walker, Name);
			walker.DivideAsset(this);
			this.WalkPrimitiveField(walker, FileExtension ?? "", nameof(FileExtension));
			walker.DivideAsset(this);
			this.WalkPPtrField(walker, Root);
			walker.DivideAsset(this);
			this.WalkPPtrListField(walker, Children);
			walker.ExitAsset(this);
		}
	}
}
