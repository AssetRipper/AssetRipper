namespace AssetRipper.Core.Classes.Mesh
{
	/// <summary>
	/// Format of the mesh index buffer data<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum IndexFormat
	{
		/// <summary>
		/// 16 bit mesh index buffer format
		/// </summary>
		UInt16 = 0,
		/// <summary>
		/// 32 bit mesh index buffer format
		/// </summary>
		UInt32 = 1,
	}
}
