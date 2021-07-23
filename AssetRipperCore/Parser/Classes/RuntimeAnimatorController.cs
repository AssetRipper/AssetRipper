using AssetRipper.Converters.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes
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

		public abstract bool IsContainsAnimationClip(AnimationClip.AnimationClip clip);

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			return base.ExportYAMLRoot(container);
		}
	}
}
