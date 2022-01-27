using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes
{
	public sealed class TransformAnimationTrack : BaseAnimationTrack
	{
		public TransformAnimationTrack(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Curves0.Read(reader);
			Curves1.Read(reader);
			Curves2.Read(reader);
			Curves3.Read(reader);
			Curves4.Read(reader);
			Curves5.Read(reader);
			Curves6.Read(reader);
		}

		public AnimationCurveTpl<Float> Curves0 = new();
		public AnimationCurveTpl<Float> Curves1 = new();
		public AnimationCurveTpl<Float> Curves2 = new();
		public AnimationCurveTpl<Float> Curves3 = new();
		public AnimationCurveTpl<Float> Curves4 = new();
		public AnimationCurveTpl<Float> Curves5 = new();
		public AnimationCurveTpl<Float> Curves6 = new();
	}
}
