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

	public Utf8String Name { get => Root.Name; set => Root.Name = value; }

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

	public override string? OriginalPath { get => Root.OriginalPath; set => Root.OriginalPath = value; }
	public override string? OverridePath { get => Root.OverridePath; set => Root.OverridePath = value; }
	public override string? OriginalDirectory { get => Root.OriginalDirectory; set => Root.OriginalDirectory = value; }
	public override string? OverrideDirectory { get => Root.OverrideDirectory; set => Root.OverrideDirectory = value; }
	public override string? OriginalName { get => Root.OriginalName; set => Root.OriginalName = value; }
	public override string? OverrideName { get => Root.OverrideName; set => Root.OverrideName = value; }
	public override string? OriginalExtension { get => Root.OriginalExtension; set => Root.OriginalExtension = value; }
	public override string? OverrideExtension { get => Root.OverrideExtension; set => Root.OverrideExtension = value; }
	public override string? AssetBundleName { get => Root.AssetBundleName; set => Root.AssetBundleName = value; }
}
