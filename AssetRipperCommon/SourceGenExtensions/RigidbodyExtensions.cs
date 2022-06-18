using AssetRipper.Core.Classes.Rigidbody;
using AssetRipper.SourceGenerated.Classes.ClassID_54;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class RigidbodyExtensions
	{
		public static RigidbodyInterpolation GetInterpolate(this IRigidbody body)
		{
			return (RigidbodyInterpolation)body.Interpolate_C54;
		}

		public static RigidbodyConstraints GetConstraints(this IRigidbody body)
		{
			if (body.Has_FreezeRotation_C54())
			{
				return body.FreezeRotation_C54 ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
			}
			else
			{
				return (RigidbodyConstraints)body.Constraints_C54;
			}
		}

		public static CollisionDetectionMode GetCollisionDetection(this IRigidbody body)
		{
			return (CollisionDetectionMode)body.CollisionDetection_C54;
		}
	}
}
