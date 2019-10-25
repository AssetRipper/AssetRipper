using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes
{
	public sealed class TransformAnimationTrack : BaseAnimationTrack
	{
		public TransformAnimationTrack(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

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

		public AnimationCurveTpl<Float> Curves0;
		public AnimationCurveTpl<Float> Curves1;
		public AnimationCurveTpl<Float> Curves2;
		public AnimationCurveTpl<Float> Curves3;
		public AnimationCurveTpl<Float> Curves4;
		public AnimationCurveTpl<Float> Curves5;
		public AnimationCurveTpl<Float> Curves6;
	}
}
