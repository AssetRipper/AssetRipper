namespace AssetRipper.Core.Classes.Renderer
{
	/// <summary>
	/// The type of motion vectors that should be generated.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum MotionVectorGenerationMode : byte
	{
		/// <summary>
		/// Use only camera movement to track motion.
		/// </summary>
		Camera = 0,
		/// <summary>
		/// Use a specific pass (if required) to track motion.
		/// </summary>
		Object = 1,
		/// <summary>
		/// Do not track motion. Motion vectors will be 0.
		/// </summary>
		ForceNoMotion = 2,
	}
}
