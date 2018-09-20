using System;

namespace uTinyRipper.Classes.Rigidbody2Ds
{
	[Flags]
	public enum RigidbodyConstraints2D
	{
		/// <summary>
		/// No constraints
		/// </summary>
		None				= 0,
		/// <summary>
		/// Freeze motion along the X-axis.
		/// </summary>
		FreezePositionX		= 1,
		/// <summary>
		/// Freeze motion along the Y-axis.
		/// </summary>
		FreezePositionY		= 2,
		/// <summary>
		/// Freeze rotation along the Z-axis.
		/// </summary>
		FreezeRotation		= 4,

		/// <summary>
		/// Freeze motion along all axes.
		/// </summary>
		FreezePosition = FreezePositionX | FreezePositionY,
		/// <summary>
		/// Freeze rotation and motion along all axes.
		/// </summary>
		FreezeAll = FreezePosition | FreezeRotation,
	}
}
