namespace UtinyRipper.Classes.AnimationClips
{
	public struct HumanGoal : IAssetReadable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadHints(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public void Read(AssetStream stream)
		{
			X.Read(stream);

			WeightT = stream.ReadSingle();
			WeightR = stream.ReadSingle();
			if(IsReadHints(stream.Version))
			{
				if(IsVector3(stream.Version))
				{
					HintT.Read3(stream);
				}
				else
				{
					HintT.Read(stream);
				}
				HintWeightT = stream.ReadSingle();
			}
		}
		
		public float WeightT { get; private set; }
		public float WeightR { get; private set; }
		public float HintWeightT { get; private set; }

		public XForm X;
		public Vector4f HintT;
	}
}
