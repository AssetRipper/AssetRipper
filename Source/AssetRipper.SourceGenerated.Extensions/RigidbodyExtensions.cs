using AssetRipper.SourceGenerated.Classes.ClassID_54;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class RigidbodyExtensions
{
	public static RigidbodyInterpolation GetInterpolate(this IRigidbody body)
	{
		return (RigidbodyInterpolation)body.Interpolate;
	}

	public static RigidbodyConstraints GetConstraints(this IRigidbody body)
	{
		//if (body.Has_FreezeRotation())
		{
			//return body.FreezeRotation ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
		}
		//else
		{
			return (RigidbodyConstraints)body.Constraints;
		}
	}

	public static CollisionDetectionMode GetCollisionDetection(this IRigidbody body)
	{
		return (CollisionDetectionMode)body.CollisionDetection;
	}
}
