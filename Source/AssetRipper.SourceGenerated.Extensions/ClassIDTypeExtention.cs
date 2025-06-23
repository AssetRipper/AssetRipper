namespace AssetRipper.SourceGenerated.Extensions;

public static class ClassIDTypeExtention
{
	public static int ToInt(this ClassIDType _this)
	{
		return (int)_this;
	}

	/// <summary>
	/// Transform and RectTransform
	/// </summary>
	public static bool IsTransform(this ClassIDType _this)
	{
		return _this is ClassIDType.Transform or ClassIDType.RectTransform;
	}

	/// <summary>
	/// Classes that inherit from LevelGameManager
	/// </summary>
	public static bool IsSceneSettings(this ClassIDType _this)
	{
		return _this is ClassIDType.OcclusionCullingSettings or ClassIDType.RenderSettings or ClassIDType.LightmapSettings or ClassIDType.NavMeshSettings;
	}
}
