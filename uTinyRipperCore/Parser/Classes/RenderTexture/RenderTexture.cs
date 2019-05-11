using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.RenderTextures;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class RenderTexture : Texture
	{
		public RenderTexture(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool IsReadIsPowerOfTwo(Version version)
		{
			return version.IsLess(3, 5);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadAntiAliasing(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool IsReadMipCount(Version version)
		{
			return version.IsGreaterEqual(2019, 2);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadColorFormat(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 2.0.0 to 4.0.0 exclusive
		/// </summary>
		public static bool IsReadIsCubemap(Version version)
		{
			return version.IsGreaterEqual(2) && version.IsLess(4);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadMipMap(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadGenerateMips(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadSRGB(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadUseDynamicScale(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadEnableCompatibleFormat(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadDimension(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		public static bool IsReadIsPowerOfTwoFirst(Version version)
		{
			return version.IsLess(2, 1);
		}
		
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}

		private static int GetSerializedVersion(Version version)
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadIsPowerOfTwo(reader.Version))
			{
				if (IsReadIsPowerOfTwoFirst(reader.Version))
				{
					IsPowerOfTwo = reader.ReadBoolean();
				}
			}
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			if (IsReadAntiAliasing(reader.Version))
			{
				AntiAliasing = reader.ReadInt32();
			}
			if (IsReadMipCount(reader.Version))
			{
				MipCount = reader.ReadInt32();
			}
			DepthFormat = reader.ReadInt32();
			if (IsReadColorFormat(reader.Version))
			{
				ColorFormat = (RenderTextureFormat)reader.ReadInt32();
			}
			if (IsReadIsPowerOfTwo(reader.Version))
			{
				if (!IsReadIsPowerOfTwoFirst(reader.Version))
				{
					IsPowerOfTwo = reader.ReadBoolean();
				}
			}
			if (IsReadIsCubemap(reader.Version))
			{
				IsCubemap = reader.ReadBoolean();
			}
			if (IsReadMipMap(reader.Version))
			{
				MipMap = reader.ReadBoolean();
			}
			if (IsReadGenerateMips(reader.Version))
			{
				GenerateMips = reader.ReadBoolean();
			}
			if (IsReadSRGB(reader.Version))
			{
				SRGB = reader.ReadBoolean();
			}
			if (IsReadUseDynamicScale(reader.Version))
			{
				UseDynamicScale = reader.ReadBoolean();
				BindMS = reader.ReadBoolean();
			}
			if (IsReadEnableCompatibleFormat(reader.Version))
			{
				EnableCompatibleFormat = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			
			TextureSettings.Read(reader);
			if (IsReadDimension(reader.Version))
			{
				Dimension = reader.ReadInt32();
				VolumeDepth = reader.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(AntiAliasingName, AntiAliasing);
			if (IsReadMipCount(container.ExportVersion))
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
			if (IsReadEnableCompatibleFormat(container.ExportVersion))
			{
				node.Add(EnableCompatibleFormatName, GetEnableCompatibleFormat(container.Version));
			}
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(DimensionName, Dimension);
			node.Add(VolumeDepthName, VolumeDepth);
			return node;
		}

		private bool GetEnableCompatibleFormat(Version version)
		{
			return IsReadEnableCompatibleFormat(version) ? EnableCompatibleFormat : true;
		}

		public override string ExportExtension => "renderTexture";

		public bool IsPowerOfTwo { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int AntiAliasing { get; private set; }
		public int MipCount { get; private set; }
		/// <summary>
		/// Depth previously
		/// </summary>
		public int DepthFormat { get; private set; }
		public RenderTextureFormat ColorFormat { get; private set; }
		public bool IsCubemap { get; private set; }
		public bool MipMap { get; private set; }
		public bool GenerateMips { get; private set; }
		public bool SRGB { get; private set; }
		public bool UseDynamicScale { get; private set; }
		public bool BindMS { get; private set; }
		public bool EnableCompatibleFormat { get; private set; }
		public int Dimension { get; private set; }
		public int VolumeDepth { get; private set; }

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

		public TextureSettings TextureSettings;
	}
}
