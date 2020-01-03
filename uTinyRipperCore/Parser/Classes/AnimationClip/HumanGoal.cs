using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct HumanGoal : IAssetReadable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasHints(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			X.Read(reader);

			WeightT = reader.ReadSingle();
			WeightR = reader.ReadSingle();
			if (HasHints(reader.Version))
			{
				if(IsVector3(reader.Version))
				{
					HintT = reader.ReadAsset<Vector3f>();
				}
				else
				{
					HintT.Read(reader);
				}
				HintWeightT = reader.ReadSingle();
			}
		}
		
		public float WeightT { get; set; }
		public float WeightR { get; set; }
		public float HintWeightT { get; set; }

		public XForm X;
		public Vector4f HintT;
	}
}
