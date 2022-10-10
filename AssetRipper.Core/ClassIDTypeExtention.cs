using AssetRipper.SourceGenerated;

namespace AssetRipper.Core
{
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
			return _this == ClassIDType.Transform || _this == ClassIDType.RectTransform;
		}

		/// <summary>
		/// Classes that inherit from LevelGameManager
		/// </summary>
		public static bool IsSceneSettings(this ClassIDType _this)
		{
			switch (_this)
			{
				case ClassIDType.OcclusionCullingSettings:
				case ClassIDType.RenderSettings:
				case ClassIDType.LightmapSettings:
				case ClassIDType.NavMeshSettings:
					return true;
			}
			return false;
		}
	}
}
