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
			node.Add("m_Width", Width);
			node.Add("m_Height", Height);
			node.Add("m_AntiAliasing", AntiAliasing);
			node.Add("m_DepthFormat", DepthFormat);
			node.Add("m_ColorFormat", (int)ColorFormat);
			node.Add("m_MipMap", MipMap);
			node.Add("m_GenerateMips", GenerateMips);
			node.Add("m_SRGB", SRGB);
			node.Add("m_UseDynamicScale", UseDynamicScale);
			node.Add("m_BindMS", BindMS);
			node.Add("m_TextureSettings", TextureSettings.ExportYAML(container));
			node.Add("m_Dimension", Dimension);
			node.Add("m_VolumeDepth", VolumeDepth);
			return node;
		}

		public override string ExportExtension => "renderTexture";

		public bool IsPowerOfTwo { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int AntiAliasing { get; private set; }
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
		public int Dimension { get; private set; }
		public int VolumeDepth { get; private set; }

		public TextureSettings TextureSettings;
	}
}
