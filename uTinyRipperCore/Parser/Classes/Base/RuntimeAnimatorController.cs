using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class RuntimeAnimatorController : NamedObject
	{
		protected RuntimeAnimatorController(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
		}

		public abstract bool IsContainsAnimationClip(AnimationClip clip);

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			return base.ExportYAMLRoot(container);
		}
	}
}
