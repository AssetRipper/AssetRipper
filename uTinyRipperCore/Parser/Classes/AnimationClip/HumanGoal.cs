namespace uTinyRipper.Classes.AnimationClips
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

		public void Read(AssetReader reader)
		{
			X.Read(reader);

			WeightT = reader.ReadSingle();
			WeightR = reader.ReadSingle();
			if(IsReadHints(reader.Version))
			{
				if(IsVector3(reader.Version))
				{
					HintT.Read3(reader);
				}
				else
				{
					HintT.Read(reader);
				}
				HintWeightT = reader.ReadSingle();
			}
		}
		
		public float WeightT { get; private set; }
		public float WeightR { get; private set; }
		public float HintWeightT { get; private set; }

		public XForm X;
		public Vector4f HintT;
	}
}
