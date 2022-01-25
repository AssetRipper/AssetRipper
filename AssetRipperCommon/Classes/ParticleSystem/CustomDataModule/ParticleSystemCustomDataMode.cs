namespace AssetRipper.Core.Classes.ParticleSystem.CustomDataModule
{
	/// <summary>
	/// Which mode the Custom Data module uses to generate its data.
	/// </summary>
	public enum ParticleSystemCustomDataMode
	{
		/// <summary>
		/// Don't generate any data.
		/// </summary>
		Disabled = 0,
		/// <summary>
		/// Generate data using ParticleSystem.MinMaxCurve.
		/// </summary>
		// Token: 0x04000412 RID: 1042
		Vector = 1,
		/// <summary>
		/// Generate data using ParticleSystem.MinMaxGradient.
		/// </summary>
		Color = 2,
	}
}
