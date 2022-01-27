using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.RenderTexture
{
	public sealed class RenderTexture : Texture
	{
		public RenderTexture(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// unknown
			if (version.IsGreaterEqual(2019))
			{
				return 3;
			}

			// Added EnableCompatibleFormat which changes the way Formats values are set
			// return 2;

			return 1;
		}

		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasIsPowerOfTwo(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasAntiAliasing(UnityVersion version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool HasMipCount(UnityVersion version) => version.IsGreaterEqual(2019, 2);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasColorFormat(UnityVersion version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 2.0.0 to 4.0.0 exclusive
		/// </summary>
		public static bool HasIsCubemap(UnityVersion version) => version.IsGreaterEqual(2) && version.IsLess(4);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasMipMap(UnityVersion version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasGenerateMips(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasSRGB(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasUseDynamicScale(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasEnableCompatibleFormat(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasDimension(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		public static bool HasIsPowerOfTwoFirst(UnityVersion version) => version.IsLess(2, 1);

		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasShadowSamplingMode(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasIsPowerOfTwo(reader.Version))
			{
				if (HasIsPowerOfTwoFirst(reader.Version))
				{
					IsPowerOfTwo = reader.ReadBoolean();
				}
			}
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			if (HasAntiAliasing(reader.Version))
			{
				AntiAliasing = reader.ReadInt32();
			}
			if (HasMipCount(reader.Version))
			{
				MipCount = reader.ReadInt32();
			}
			DepthFormat = reader.ReadInt32();
			if (HasColorFormat(reader.Version))
			{
				ColorFormat = (RenderTextureFormat)reader.ReadInt32();
			}
			if (HasIsPowerOfTwo(reader.Version))
			{
				if (!HasIsPowerOfTwoFirst(reader.Version))
				{
					IsPowerOfTwo = reader.ReadBoolean();
				}
			}
			if (HasIsCubemap(reader.Version))
			{
				IsCubemap = reader.ReadBoolean();
			}
			if (HasMipMap(reader.Version))
			{
				MipMap = reader.ReadBoolean();
			}
			if (HasGenerateMips(reader.Version))
			{
				GenerateMips = reader.ReadBoolean();
			}
			if (HasSRGB(reader.Version))
			{
				SRGB = reader.ReadBoolean();
			}
			if (HasUseDynamicScale(reader.Version))
			{
				UseDynamicScale = reader.ReadBoolean();
				BindMS = reader.ReadBoolean();
			}
			if (HasEnableCompatibleFormat(reader.Version))
			{
				EnableCompatibleFormat = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			TextureSettings.Read(reader);
			if (HasDimension(reader.Version))
			{
				Dimension = reader.ReadInt32();
				VolumeDepth = reader.ReadInt32();
			}

			if (HasShadowSamplingMode(reader.Version))
			{
				reader.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(AntiAliasingName, AntiAliasing);
			if (HasMipCount(container.ExportVersion))
			{
				node.Add(MipCountName, MipCount);
			}
			node.Add(DepthFormatName, DepthFormat);
			node.Add(ColorFormatName, (int)ColorFormat);
			node.Add(MipMapName, MipMap);
			node.Add(GenerateMipsName, GenerateMips);
			node.Add(SRGBName, SRGB);
			node.Add(UseDynamicScaleName, UseDynamicScale);
			node.Add(BindMSName, BindMS);
			if (HasEnableCompatibleFormat(container.ExportVersion))
			{
				node.Add(EnableCompatibleFormatName, GetEnableCompatibleFormat(container.Version));
			}
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(DimensionName, Dimension);
			node.Add(VolumeDepthName, VolumeDepth);
			return node;
		}

		private bool GetEnableCompatibleFormat(UnityVersion version)
		{
			return HasEnableCompatibleFormat(version) ? EnableCompatibleFormat : true;
		}

		public override string ExportExtension => "renderTexture";

		public bool IsPowerOfTwo { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int AntiAliasing { get; set; }
		public int MipCount { get; set; }
		/// <summary>
		/// Depth previously
		/// </summary>
		public int DepthFormat { get; set; }
		public RenderTextureFormat ColorFormat { get; set; }
		public bool IsCubemap { get; set; }
		public bool MipMap { get; set; }
		public bool GenerateMips { get; set; }
		public bool SRGB { get; set; }
		public bool UseDynamicScale { get; set; }
		public bool BindMS { get; set; }
		public bool EnableCompatibleFormat { get; set; }
		public int Dimension { get; set; }
		public int VolumeDepth { get; set; }

		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string AntiAliasingName = "m_AntiAliasing";
		public const string MipCountName = "m_MipCount";
		public const string DepthFormatName = "m_DepthFormat";
		public const string ColorFormatName = "m_ColorFormat";
		public const string MipMapName = "m_MipMap";
		public const string GenerateMipsName = "m_GenerateMips";
		public const string SRGBName = "m_SRGB";
		public const string UseDynamicScaleName = "m_UseDynamicScale";
		public const string BindMSName = "m_BindMS";
		public const string EnableCompatibleFormatName = "m_EnableCompatibleFormat";
		public const string TextureSettingsName = "m_TextureSettings";
		public const string DimensionName = "m_Dimension";
		public const string VolumeDepthName = "m_VolumeDepth";

		public GLTextureSettings TextureSettings = new();
	}
}
