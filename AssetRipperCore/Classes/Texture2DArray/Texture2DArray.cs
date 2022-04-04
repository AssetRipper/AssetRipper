using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Core.Classes.Texture2DArray
{
	public class Texture2DArray : Texture
	{
		public Texture2DArray(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasStreamData(UnityVersion version) => version.IsGreaterEqual(5, 6, 0);
		/// <summary>
		/// 2020.2.0a12 and greater
		/// </summary>
		public static bool HasUsageMode(UnityVersion version) => version.IsGreaterEqual(2020, 2, 0, UnityVersionType.Alpha, 12);

		public static int ToSerializedVersion(UnityVersion version) => 2;
		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			if (reader.Version.IsGreaterEqual(2019, 1, 0))
			{
				ColorSpace = (ColorSpace)reader.ReadInt32();
				Format = (GraphicsFormat)reader.ReadInt32();
			}
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			Depth = reader.ReadInt32();
			if (reader.Version.IsLess(2019, 1, 0))
				Format = (GraphicsFormat)reader.ReadInt32();
			MipCount = reader.ReadInt32();
			DataSize = reader.ReadInt32();
			TextureSettings.Read(reader);
			if (reader.Version.IsLess(2019, 1, 0))
				ColorSpace = (ColorSpace)reader.ReadInt32();
			if (HasUsageMode(reader.Version))
				UsageMode = reader.ReadInt32();
			IsReadable = reader.ReadInt32();
			ImageData = reader.ReadByteArray();
			reader.AlignStream();
			if (HasStreamData(reader.Version))
				StreamData.Read(reader);
		}
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.ForceAddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (container.ExportVersion.IsGreaterEqual(2019, 1, 0))
			{
				node.Add(ColorSpaceName, (int)ColorSpace);
				node.Add(FormatName, (int)Format);
			}
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(DepthName, Depth);
			if (container.ExportVersion.IsLess(2019, 1, 0))
				node.Add(FormatName, (int)Format);
			node.Add(MipCountName, MipCount);
			node.Add(DataSizeName, DataSize);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			if (container.ExportVersion.IsLess(2019, 1, 0))
				node.Add(ColorSpaceName, (int)ColorSpace);
			if (HasUsageMode(container.ExportVersion))
				node.Add(UsageModeName, UsageMode);
			node.Add(IsReadableName, IsReadable);
			byte[] imageData = GetExportImageData();
			node.Add(ImageDataName, imageData.Length);
			node.Add(Layout.LayoutInfo.TypelessdataName, imageData.ExportYAML());
			StreamingInfo streamData = new StreamingInfo();
			node.Add(StreamDataName, streamData.ExportYAML(container));
			return node;
		}

		private byte[] GetImageData()
		{
			byte[] data = ImageData;

			if (!data.IsNullOrEmpty())
				return ImageData;
			else if (StreamData != null && StreamData.IsSet())
				data = StreamData.GetContent(SerializedFile);

			return data ?? Array.Empty<byte>();
		}

		private bool CheckAssetIntegrity()
		{
			if (!ImageData.IsNullOrEmpty())
				return true;
			else if (StreamData != null)
				return StreamData.CheckIntegrity(SerializedFile);
			else
				return false;
		}

		private byte[] GetExportImageData()
		{
			if (CheckAssetIntegrity())
				return GetImageData();

			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{this.GetValidName()}' because resources file '{StreamData.Path}' wasn't found");
			return Array.Empty<byte>();
		}

		public ColorSpace ColorSpace { get; set; }
		public GraphicsFormat Format { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Depth { get; set; }
		public int MipCount { get; set; }
		public int DataSize { get; set; }
		public IGLTextureSettings TextureSettings { get; } = new GLTextureSettings();
		public int UsageMode { get; set; }
		public int IsReadable { get; set; }
		public byte[] ImageData { get; set; }
		public IStreamingInfo StreamData { get; } = new StreamingInfo();

		public const string ColorSpaceName = "m_ColorSpace";
		public const string FormatName = "m_Format";
		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string DepthName = "m_Depth";
		public const string MipCountName = "m_MipCount";
		public const string DataSizeName = "m_DataSize";
		public const string TextureSettingsName = "m_TextureSettings";
		public const string UsageModeName = "m_UsageMode";
		public const string IsReadableName = "m_IsReadable";
		public const string ImageDataName = "image data";
		public const string StreamDataName = "m_StreamData";
	}
}
