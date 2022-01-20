﻿namespace AssetRipper.Core.Classes.Flare
{
	/// <summary>
	/// Enum describing how the individual Flare Element images are laid out inside the Flare Texture.<br/>
	/// <see href="https://docs.unity3d.com/Manual/class-Flare.html"/>
	/// </summary>
	public enum TextureLayout
	{
		OneLargeFourSmall = 0,
		OneLargeTwoMediumEightSmall = 1,
		OneTexture = 2,
		TwoGrid = 3,
		ThreeGrid = 4,
		FourGrid = 5
	}
}