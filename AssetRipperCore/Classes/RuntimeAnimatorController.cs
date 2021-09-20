using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public abstract class RuntimeAnimatorController : NamedObject
	{
		protected RuntimeAnimatorController(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
		}

		public abstract bool IsContainsAnimationClip(AnimationClip.AnimationClip clip);

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			return base.ExportYAMLRoot(container);
		}
	}
}
