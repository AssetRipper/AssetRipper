using AssetRipper.Classes.Misc;
using AssetRipper.Classes.Misc.Serializable;
using AssetRipper.Classes.Utils.Extensions;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.Math;

namespace AssetRipper.Classes.AnimationClip
{
	public class HumanPose : IAssetReadable
	{
		public HumanPose() { }

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3f(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasTArray(Version version) => version.IsGreaterEqual(5, 2);

		public void Read(AssetReader reader)
		{
			RootX.Read(reader);
			if (IsVector3f(reader.Version))
			{
				LookAtPosition = reader.ReadAsset<Vector3f>();
			}
			else
			{
				LookAtPosition.Read(reader);
			}
			LookAtWeight.Read(reader);
			GoalArray = reader.ReadAssetArray<HumanGoal>();
			LeftHandPose.Read(reader);
			RightHandPose.Read(reader);
			DoFArray = reader.ReadSingleArray();

			if (HasTArray(reader.Version))
			{
				if (IsVector3f(reader.Version))
				{
					TDoFArray = reader.ReadVector3Array();
				}
				else
				{
					TDoFArray = reader.ReadAssetArray<Vector4f>();
				}
			}
		}

		public HumanGoal[] GoalArray { get; set; }
		public float[] DoFArray { get; set; }
		public Vector4f[] TDoFArray { get; set; }

		public XForm RootX = new();
		public Vector4f LookAtPosition;
		public Vector4f LookAtWeight;
		public HandPose LeftHandPose = new();
		public HandPose RightHandPose = new();
	}
}
