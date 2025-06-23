using AssetRipper.SourceGenerated.Classes.ClassID_50;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Rigidbody2DExtensions
{
	public static RigidbodyType2D GetBodyType(this IRigidbody2D body)
	{
		if (body.Has_IsKinematic())
		{
			return body.IsKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Static;
		}
		else
		{
			return body.BodyTypeE;
		}
	}

	public static RigidbodyInterpolation2D GetInterpolate(this IRigidbody2D body)
	{
		if (body.Has_Interpolate_Byte())
		{
			return (RigidbodyInterpolation2D)body.Interpolate_Byte;
		}
		else
		{
			return (RigidbodyInterpolation2D)body.Interpolate_Int32;
		}
	}

	public static RigidbodySleepMode2D GetSleepingMode(this IRigidbody2D body)
	{
		if (body.Has_SleepingMode_Byte())
		{
			return (RigidbodySleepMode2D)body.SleepingMode_Byte;
		}
		else
		{
			return (RigidbodySleepMode2D)body.SleepingMode_Int32;
		}
	}

	public static CollisionDetectionMode2D GetCollisionDetection(this IRigidbody2D body)
	{
		if (body.Has_CollisionDetection_Byte())
		{
			return (CollisionDetectionMode2D)body.CollisionDetection_Byte;
		}
		else
		{
			return (CollisionDetectionMode2D)body.CollisionDetection_Int32;
		}
	}

	public static RigidbodyConstraints2D GetConstraints(this IRigidbody2D body)
	{
		if (body.Has_FixedAngle())
		{
			return body.FixedAngle ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
		}
		else
		{
			return body.ConstraintsE;
		}
	}
}
