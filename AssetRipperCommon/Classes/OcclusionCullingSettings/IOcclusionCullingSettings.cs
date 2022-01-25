using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.OcclusionCullingData;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.OcclusionCullingSettings
{
	public interface IOcclusionCullingSettings : IUnityObjectBase
	{
		public byte[] PVSData { get; set; }
		UnityGUID SceneGUID { get; set; }
		/// <summary>
		/// m_PVSObjectsArray previously
		/// </summary>
		PPtr<Renderer.IRenderer>[] StaticRenderers { get; }
		/// <summary>
		/// m_PVSPortalsArray previously
		/// </summary>
		PPtr<IOcclusionPortal>[] Portals { get; }
		PPtr<IOcclusionCullingData> OcclusionCullingData { get; }
	}

	public static class OcclusionCullingSettingsExtensions
	{
		/// <summary>
		/// 3.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasReadPVSData(UnityVersion version) => version.IsGreaterEqual(3) && version.IsLess(5, 5);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasSceneGUID(UnityVersion version) => version.IsGreaterEqual(5, 5);
	}
}