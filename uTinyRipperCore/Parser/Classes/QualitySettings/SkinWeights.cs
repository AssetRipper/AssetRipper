namespace uTinyRipper.Classes.QualitySettingss
{
	/// <summary>
	/// How many bones affect each vertex.
	/// BlendWeights previously
	/// </summary>
	public enum SkinWeights
	{
		/// <summary>
		/// One bone affects each vertex.
		/// </summary>
		OneBone		= 1,
		/// <summary>
		/// Two bones affect each vertex.
		/// </summary>
		TwoBones	= 2,
		/// <summary>
		/// Four bones affect each vertex.
		/// </summary>
		FourBones	= 4,
		/// <summary>
		/// An unlimited number of bones affect each vertex
		/// </summary>
		Unlimited	= 255,
	}
}
