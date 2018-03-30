using UtinyRipper.Classes.AnimationClips;

namespace UtinyRipper.Classes
{
	public sealed class TransformAnimationTrack : BaseAnimationTrack
	{
		public TransformAnimationTrack(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Curves0.Read(stream);
			Curves1.Read(stream);
			Curves2.Read(stream);
			Curves3.Read(stream);
			Curves4.Read(stream);
			Curves5.Read(stream);
			Curves6.Read(stream);
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
