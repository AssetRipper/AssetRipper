using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.SourceGenerated.Classes.ClassID_30;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GraphicsSettingsExtensions
	{
		public static TransparencySortMode GetTransparencySortMode(this IGraphicsSettings settings)
		{
			return (TransparencySortMode)settings.TransparencySortMode_C30;
		}

		/// <summary>
		/// Default: <see cref="RenderingPath.Forward"/>
		/// </summary>
		public static RenderingPath GetDefaultRenderingPath(this IGraphicsSettings settings)
		{
			return (RenderingPath)settings.DefaultRenderingPath_C30;
		}

		/// <summary>
		/// Default: <see cref="RenderingPath.Forward"/>
		/// </summary>
		public static RenderingPath GetDefaultMobileRenderingPath(this IGraphicsSettings settings)
		{
			return (RenderingPath)settings.DefaultMobileRenderingPath_C30;
		}

		/// <summary>
		/// Default: <see cref="LightmapStrippingMode.Automatic"/>
		/// </summary>
		public static LightmapStrippingMode GetLightmapStripping(this IGraphicsSettings settings)
		{
			return (LightmapStrippingMode)settings.LightmapStripping_C30;
		}

		/// <summary>
		/// Default: <see cref="LightmapStrippingMode.Automatic"/>
		/// </summary>
		public static LightmapStrippingMode GetFogStripping(this IGraphicsSettings settings)
		{
			return (LightmapStrippingMode)settings.FogStripping_C30;
		}

		/// <summary>
		/// Default: <see cref="InstancingStrippingVariant.StripUnused"/>
		/// </summary>
		public static InstancingStrippingVariant GetInstancingStripping(this IGraphicsSettings settings)
		{
			return (InstancingStrippingVariant)settings.InstancingStripping_C30;
		}
	}
}
