using AssetRipper.Core.Classes.Rigidbody2D;
using AssetRipper.SourceGenerated.Classes.ClassID_50;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Rigidbody2DExtensions
	{
		public static RigidbodyType2D GetBodyType(this IRigidbody2D body)
		{
			if (body.Has_IsKinematic_C50())
			{
				return body.IsKinematic_C50 ? RigidbodyType2D.Kinematic : RigidbodyType2D.Static;
			}
			else
			{
				return (RigidbodyType2D)body.BodyType_C50;
			}
		}

		public static RigidbodyInterpolation2D GetInterpolate(this IRigidbody2D body)
		{
			if (body.Has_Interpolate_C50_Byte())
			{
				return (RigidbodyInterpolation2D)body.Interpolate_C50_Byte;
			}
			else
			{
				return (RigidbodyInterpolation2D)body.Interpolate_C50_Int32;
			}
		}

		public static RigidbodySleepMode2D GetSleepingMode(this IRigidbody2D body)
		{
			if (body.Has_SleepingMode_C50_Byte())
			{
				return (RigidbodySleepMode2D)body.SleepingMode_C50_Byte;
			}
			else
			{
				return (RigidbodySleepMode2D)body.SleepingMode_C50_Int32;
			}
		}

		public static CollisionDetectionMode2D GetCollisionDetection(this IRigidbody2D body)
		{
			if (body.Has_CollisionDetection_C50_Byte())
			{
				return (CollisionDetectionMode2D)body.CollisionDetection_C50_Byte;
			}
			else
			{
				return (CollisionDetectionMode2D)body.CollisionDetection_C50_Int32;
			}
		}

		public static RigidbodyConstraints2D GetConstraints(this IRigidbody2D body)
		{
			if (body.Has_FixedAngle_C50())
			{
				return body.FixedAngle_C50 ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
			}
			else
			{
				return (RigidbodyConstraints2D)body.Constraints_C50;
			}
		}
	}
}
