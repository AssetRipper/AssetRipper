using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct HandPose : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			GrabX.Read(reader);

			DoFArray = reader.ReadSingleArray();
			Override = reader.ReadSingle();
			CloseOpen = reader.ReadSingle();
			InOut = reader.ReadSingle();
			Grab = reader.ReadSingle();
		}

		public float[] DoFArray { get; set; }
		public float Override { get; set; }
		public float CloseOpen { get; set; }
		public float InOut { get; set; }
		public float Grab { get; set; }

		public XForm GrabX;
	}
}
