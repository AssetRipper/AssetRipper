using AssetRipper.Core.Classes.Misc.Bones;
using AssetRipper.SourceGenerated.Subclasses.Human;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class HumanExtensions
	{
		public static void UpdateBoneArray(this IHuman human, UnityVersion version)
		{
			human.HumanBoneIndex = UpdateBoneArray(human.HumanBoneIndex, version);
			human.HumanBoneMass = UpdateBoneArray(human.HumanBoneMass, version);
			if (human.Has_ColliderIndex())
			{
				human.ColliderIndex = UpdateBoneArray(human.ColliderIndex, version);
			}
		}

		private static int[] UpdateBoneArray(int[] array, UnityVersion version)
		{
			if (!BoneTypeExtensions.IsIncludeUpperChest(version))
			{
				int[] fixedArray = new int[array.Length + 1];
				BoneType bone;
				for (bone = BoneType.Hips; bone < BoneType.UpperChest; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone];
				}
				fixedArray[(int)bone] = -1;
				for (bone = BoneType.UpperChest + 1; bone < BoneType.Last; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone - 1];
				}
				return fixedArray;
			}
			return array;
		}

		private static float[] UpdateBoneArray(float[] array, UnityVersion version)
		{
			if (!BoneTypeExtensions.IsIncludeUpperChest(version))
			{
				float[] fixedArray = new float[array.Length + 1];
				BoneType bone;
				for (bone = BoneType.Hips; bone < BoneType.UpperChest; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone];
				}
				fixedArray[(int)bone] = 0.0f;
				for (bone = BoneType.UpperChest + 1; bone < BoneType.Last; bone++)
				{
					fixedArray[(int)bone] = array[(int)bone - 1];
				}
				return fixedArray;
			}
			return array;
		}
	}
}
