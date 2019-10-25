namespace uTinyRipper.AssetExporters.Classes
{
	/// <summary>
	/// Select this to set basic parameters depending on the purpose of your texture.
	/// </summary>
	public enum TextureImporterType
	{
		/// <summary>
		/// This is the most common setting used for all the textures in general.
		/// </summary>
		Default = 0,
		/// <summary>
		/// This is the most common setting used for all the textures in general.
		/// </summary>
		Image = 0,
		/// <summary>
		/// Select this to turn the color channels into a format suitable for real-time normal mapping.
		/// </summary>
		NormalMap = 1,
		Bump = 1,
		/// <summary>
		/// Use this if your texture is going to be used on any HUD/GUI Controls.
		/// </summary>
		GUI = 2,
		Cubemap = 3,
		Reflection = 3,
		/// <summary>
		/// This sets up your texture with the basic parameters used for the Cookies of your lights.
		/// </summary>
		Cookie = 4,
		Advanced = 5,
		/// <summary>
		/// This sets up your texture with the parameters used by the lightmap.
		/// </summary>
		Lightmap = 6,
		/// <summary>
		/// Use this if your texture is going to be used as a cursor.
		/// </summary>
		Cursor = 7,
		/// <summary>
		/// Select this if you will be using your texture for Sprite graphics.
		/// </summary>
		Sprite = 8,
		HDRI = 9,
		/// <summary>
		/// Use this for texture containing a single channel.
		/// </summary>
		SingleChannel = 10,
	}
}
