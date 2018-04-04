using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.RenderTextures;
using UtinyRipper.Classes.Textures;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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
		
		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadIsPowerOfTwo(stream.Version))
			{
				if (IsReadIsPowerOfTwoFirst(stream.Version))
				{
					IsPowerOfTwo = stream.ReadBoolean();
				}
			}
			Width = stream.ReadInt32();
			Height = stream.ReadInt32();
			if (IsReadAntiAliasing(stream.Version))
			{
				AntiAliasing = stream.ReadInt32();
			}
			DepthFormat = stream.ReadInt32();
			if (IsReadColorFormat(stream.Version))
			{
				ColorFormat = (RenderTextureFormat)stream.ReadInt32();
			}
			if (IsReadIsPowerOfTwo(stream.Version))
			{
				if (!IsReadIsPowerOfTwoFirst(stream.Version))
				{
					IsPowerOfTwo = stream.ReadBoolean();
				}
			}
			if (IsReadIsCubemap(stream.Version))
			{
				IsCubemap = stream.ReadBoolean();
			}
			if (IsReadMipMap(stream.Version))
			{
				MipMap = stream.ReadBoolean();
			}
			if (IsReadGenerateMips(stream.Version))
			{
				GenerateMips = stream.ReadBoolean();
			}
			if (IsReadSRGB(stream.Version))
			{
				SRGB = stream.ReadBoolean();
			}
			if (IsReadUseDynamicScale(stream.Version))
			{
				UseDynamicScale = stream.ReadBoolean();
				BindMS = stream.ReadBoolean();
			}
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			
			TextureSettings.Read(stream);
			if (IsReadDimension(stream.Version))
			{
				Dimension = stream.ReadInt32();
				VolumeDepth = stream.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
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
			node.Add("m_TextureSettings", TextureSettings.ExportYAML(exporter));
			node.Add("m_Dimension", Dimension);
			node.Add("m_VolumeDepth", VolumeDepth);
			return node;
		}

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
