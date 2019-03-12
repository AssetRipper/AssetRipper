using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes
{
	public sealed class Texture3D : Texture
	{
		public Texture3D(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadDepth(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2.6.0 to 4.0.0 exclusize or 5.4.0 and greater
		/// </summary>
		public static bool IsReadIsReadable(Version version)
		{
			return IsReadIsReadableFirst(version) || IsReadIsReadableSecond(version);
		}
		/// <summary>
		/// From 3.0.0 to 4.0.0 exclusive
		/// </summary>
		public static bool IsReadReadAllowed(Version version)
		{
			return version.IsGreaterEqual(3) && version.IsLess(4);
		}
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsReadImageCount(Version version)
		{
			return version.IsLess(4);
		}
		/// <summary>
		/// 3.0.0 to 4.0.0 exclusive
		/// </summary>
		public static bool IsReadLightmapFormat(Version version)
		{
			return version.IsGreaterEqual(3) && version.IsLess(4);
		}
		/// <summary>
		/// 3.5.0 to 4.0.0 exclusive
		/// </summary>
		public static bool IsReadColorSpace(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(4);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadStreamData(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		private static int GetSerializedVersion(Version version)
		{
			// MipMap converted to MipCount
			if (version.IsGreaterEqual(5, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		private static bool IsReadDataSizeFirst(Version version)
		{
			return version.IsLess(4);
		}
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		private static bool IsBoolMinMap(Version version)
		{
			return version.IsLess(5, 2);
		}
		/// <summary>
		/// 2.6.0 to 4.0.0 exclusize
		/// </summary>
		private static bool IsReadIsReadableFirst(Version version)
		{
			return version.IsGreaterEqual(2, 6) && version.IsLess(4);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsReadIsReadableSecond(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			if (IsReadDepth(reader.Version))
			{
				Depth = reader.ReadInt32();
			}
			if (IsReadDataSizeFirst(reader.Version))
			{
				DataSize = reader.ReadInt32();
			}
			Format = (TextureFormat)reader.ReadInt32();

			if (IsBoolMinMap(reader.Version))
			{
				bool mipMap = reader.ReadBoolean();
				if (mipMap)
				{
					int maxSide = Math.Max(Width, Height);
					MipCount = Convert.ToInt32(Math.Log(maxSide) / Math.Log(2));
				}
				else
				{
					MipCount = 1;
				}
			}
			else
			{
				MipCount = reader.ReadInt32();
			}

			if (!IsReadDataSizeFirst(reader.Version))
			{
				DataSize = reader.ReadInt32();
			}
			if (IsReadIsReadableFirst(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
			}
			if (IsReadReadAllowed(reader.Version))
			{
				ReadAllowed = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);

			if (IsReadImageCount(reader.Version))
			{
				ImageCount = reader.ReadInt32();
				TextureDimension = (TextureDimension)reader.ReadInt32();
			}
			TextureSettings.Read(reader);

			if (IsReadIsReadableSecond(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadLightmapFormat(reader.Version))
			{
				LightmapFormat = (TextureUsageMode)reader.ReadInt32();
			}
			if (IsReadColorSpace(reader.Version))
			{
				ColorSpace = (ColorSpace)reader.ReadInt32();
			}

			m_imageData = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
			if (IsReadStreamData(reader.Version))
			{
				StreamData.Read(reader);
			}
		}

		protected sealed override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(DepthName, Depth);
			node.Add(FormatName, (int)Format);
			node.Add(MipCountName, MipCount);
			node.Add(DataSizeName, DataSize);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(IsReadableName, IsReadable);
			IReadOnlyList<byte> imageData = GetImageData(container.Version);
			node.Add(ImageDataName, imageData.Count);
			node.Add(TypelessdataName, imageData.ExportYAML());
			StreamingInfo streamData = new StreamingInfo(true);
			node.Add(StreamDataName, streamData.ExportYAML(container));
			return node;
		}

		private IReadOnlyList<byte> GetImageData(Version version)
		{
			if (IsReadStreamData(version))
			{
				string path = StreamData.Path;
				if (path != string.Empty)
				{
					if (m_imageData.Length != 0)
					{
						throw new Exception("Texture3D contains both data and resource path");
					}

					using (ResourcesFile res = File.Collection.FindResourcesFile(File, path))
					{
						if (res == null)
						{
							Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because resources file '{path}' wasn't found");
						}
						else
						{
							using (PartialStream resStream = new PartialStream(res.Stream, res.Offset, res.Size))
							{
								resStream.Position = StreamData.Offset;
								using (BinaryReader reader = new BinaryReader(resStream))
								{
									return reader.ReadBytes((int)StreamData.Size);
								}
							}
						}
					}
				}
			}

			return m_imageData;
		}

		public int Width { get; private set; }
		public int Height { get; private set; }
		/// <summary>
		/// CompleteImageSize previously
		/// </summary>
		public int DataSize { get; private set; }
		public bool IsReadable { get; private set; }
		public bool ReadAllowed { get; private set; }
		public int Depth { get; private set; }
		/// <summary>
		/// TextureFormat previously
		/// </summary>
		public TextureFormat Format { get; private set; }
		/// <summary>
		/// bool MipMap previously
		/// </summary>
		public int MipCount { get; private set; }
		public int ImageCount { get; private set; }
		public TextureDimension TextureDimension { get; private set; }
		public TextureUsageMode LightmapFormat { get; private set; }
		public ColorSpace ColorSpace { get; private set; }
		public IReadOnlyCollection<byte> ImageData => m_imageData;

		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string DepthName = "m_Depth";
		public const string CompleteImageSizeName = "m_CompleteImageSize";
		public const string FormatName = "m_Format";
		public const string TextureFormatName = "m_TextureFormat";
		public const string MipMapName = "m_MipMap";
		public const string MipCountName = "m_MipCount";
		public const string IsReadableName = "m_IsReadable";
		public const string DataSizeName = "m_DataSize";
		public const string ImageCountName = "m_ImageCount";
		public const string TextureDimensionName = "m_TextureDimension";
		public const string TextureSettingsName = "m_TextureSettings";
		public const string ImageDataName = "image data";
		public const string TypelessdataName = "_typelessdata";
		public const string StreamDataName = "m_StreamData";

		public TextureSettings TextureSettings;
		public StreamingInfo StreamData;

		private byte[] m_imageData;
	}
}
