namespace AssetRipper.Core.Classes.Rigidbody
{
	public enum RigidbodyConstraints
	{
		/// <summary>
		/// No constraints
		/// </summary>
		None = 0,
		/// <summary>
		/// Freeze motion along the X-axis.
		/// </summary>
		FreezePositionX = 0x02,
		/// <summary>
		/// Freeze motion along the Y-axis.
		/// </summary>
		FreezePositionY = 0x04,
		/// <summary>
		/// Freeze motion along the Z-axis.
		/// </summary>
		FreezePositionZ = 0x08,
		/// <summary>
		/// Freeze rotation along the X-axis.
		/// </summary>
		FreezeRotationX = 0x10,
		/// <summary>
		/// Freeze rotation along the Y-axis.
		/// </summary>
		FreezeRotationY = 0x20,
		/// <summary>
		/// Freeze rotation along the Z-axis.
		/// </summary>
		FreezeRotationZ = 0x40,
		/// <summary>
		/// Freeze motion along all axes.
		/// </summary>
		FreezePosition = 0x0e,
		/// <summary>
		/// Freeze rotation along all axes.
		/// </summary>
		FreezeRotation = 0x70,
		/// <summary>
		/// Freeze rotation and motion along all axes.
		/// </summary>
		FreezeAll = 0x7e,
	}
}
