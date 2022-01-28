using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class HumanGoal : IAssetReadable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasHints(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3f(UnityVersion version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			X.Read(reader);

			WeightT = reader.ReadSingle();
			WeightR = reader.ReadSingle();
			if (HasHints(reader.Version))
			{
				if (IsVector3f(reader.Version))
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

		public XForm X = new();
		public Vector4f HintT = new();
	}
}
