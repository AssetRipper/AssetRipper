namespace AssetRipper.Core.Classes.Light
{
	/// <summary>
	/// Enum describing what part of a light contribution can be baked.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	[Flags]
	public enum LightmapBakeType
	{
		Mixed = 1,
		Baked = 2,
		Realtime = 4,
	}
}
