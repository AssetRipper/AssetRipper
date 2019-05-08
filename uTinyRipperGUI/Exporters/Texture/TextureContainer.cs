using System;
using System.IO;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipperGUI.TextureContainers;
using uTinyRipperGUI.TextureContainers.DDS;
using uTinyRipperGUI.TextureContainers.KTX;
using uTinyRipperGUI.TextureContainers.PVR;

namespace uTinyRipperGUI.Exporters
{
	public static class TextureContainer
	{
		public static void ExportBinary(Texture2D texture, Stream exportStream)
		{
			if (texture.CompleteImageSize == 0)
			{
				return;
			}

			if (Texture2D.IsReadStreamData(texture.File.Version))
			{
				string path = texture.StreamData.Path;
				if (path != string.Empty)
				{
					if (texture.ImageData.Count != 0)
					{
						throw new Exception("Texture contains data and resource path");
					}

					IResourceFile res = texture.File.Collection.FindResourceFile(path);
					if (res == null)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.ValidName}' because resources file '{path}' wasn't found");
					}
					else
					{
						using (PartialStream resStream = new PartialStream(res.Stream, res.Offset, res.Size))
						{
							resStream.Position = texture.StreamData.Offset;
							Export(texture, exportStream, resStream, texture.StreamData.Size);
						}
					}
					return;
				}
			}

			using (MemoryStream memStream = new MemoryStream((byte[])texture.ImageData))
			{
				Export(texture, exportStream, memStream, texture.ImageData.Count);
			}
		}

		public static void Export(Texture2D texture, Stream destination, Stream source, long length)
		{
			switch (texture.TextureFormat.ToContainerType())
			{
				case ContainerType.None:
					source.CopyStream(destination, length);
					break;

				case ContainerType.DDS:
					ExportDDS(texture, destination, source, length);
					break;

				case ContainerType.PVR:
					ExportPVR(texture, destination, source, length);
					break;

				case ContainerType.KTX:
					ExportKTX(texture, destination, source, length);
					break;

				default:
					throw new NotSupportedException($"Unsupported texture container {texture.TextureFormat.ToContainerType()}");
			}
		}

		public static void ExportDDS(Texture2D texture, Stream destination, Stream source, long length)
		{
			DDSContainerParameters @params = new DDSContainerParameters()
			{
				DataLength = length,
				MipMapCount = texture.MipCount,
				Width = texture.Width,
				Height = texture.Height,
				IsPitchOrLinearSize = texture.DDSIsPitchOrLinearSize(),
				PixelFormatFlags = texture.DDSPixelFormatFlags(),
				FourCC = (DDSFourCCType)texture.DDSFourCC(),
				RGBBitCount = texture.DDSRGBBitCount(),
				RBitMask = texture.DDSRBitMask(),
				GBitMask = texture.DDSGBitMask(),
				BBitMask = texture.DDSBBitMask(),
				ABitMask = texture.DDSABitMask(),
				Caps = texture.DDSCaps(),
			};

			EndianType endianess = Texture2D.IsSwapBytes(texture.File.Platform, texture.TextureFormat) ? EndianType.BigEndian : EndianType.LittleEndian;
			using (EndianReader sourceReader = new EndianReader(source, endianess))
			{
				DDSContainer.ExportDDS(sourceReader, destination, @params);
			}
		}

		public static void ExportPVR(Texture2D texture, Stream writer, Stream reader, long length)
		{
			PVRContainerParameters @params = new PVRContainerParameters()
			{
				DataLength = length,
				PixelFormat = texture.PVRPixelFormat(),
				Width = texture.Width,
				Height = texture.Height,
				MipMapCount = texture.MipCount,
			};
			PVRContainer.ExportPVR(writer, reader, @params);
		}

		public static void ExportKTX(Texture2D texture, Stream writer, Stream reader, long length)
		{
			KTXContainerParameters @params = new KTXContainerParameters()
			{
				DataLength = length,
				InternalFormat = texture.KTXInternalFormat(),
				BaseInternalFormat = texture.KTXBaseInternalFormat(),
				Width = texture.Width,
				Height = texture.Height,
			};
			KTXContainer.ExportKXT(writer, reader, @params);
		}
	}
}
