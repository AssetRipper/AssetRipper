namespace AssetRipper.Core.Classes.Rigidbody
{
	public enum RigidbodyInterpolation : byte
	{
		/// <summary>
		/// No Interpolation.
		/// </summary>
		None = 0,
		/// <summary>
		/// Interpolation will always lag a little bit behind but can be smoother than extrapolation.
		/// </summary>
		Interpolate = 1,
		/// <summary>
		/// Extrapolation will predict the position of the rigidbody based on the current velocity.
		/// </summary>
		Extrapolate = 2
	}
}
