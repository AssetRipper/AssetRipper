using System;
using System.Collections;
using System.IO;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Meshes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.Shaders;

namespace uTinyRipper.Converters.Meshes
{
	public static class VertexDataConverter
	{
		public static VertexData Convert(IExportContainer container, Mesh originMesh)
		{
			VertexData instance = new VertexData();
			if (VertexData.HasCurrentChannels(container.ExportVersion))
			{
				instance.CurrentChannels = GetCurrentChannels(container, ref originMesh.VertexData);
			}
			instance.VertexCount = originMesh.VertexData.VertexCount;
			if (VertexData.HasChannels(container.ExportVersion))
			{
				instance.Channels = GetChannels(container, originMesh);
			}
			if (VertexData.HasStreams(container.ExportVersion))
			{
				instance.Streams = originMesh.VertexData.Streams.ToArray();
			}
			instance.Data = GetData(container, originMesh, ref instance);
			return instance;
		}

		private static uint GetCurrentChannels(IExportContainer container, ref VertexData origin)
		{
			if (ShaderChannelExtensions.ShaderChannel5Relevant(container.Version))
			{
				return origin.CurrentChannels;
			}
			else
			{
				BitArray curBits = new BitArray(BitConverter.GetBytes(origin.CurrentChannels));
				curBits.Set((int)ShaderChannel5.Tangent, curBits.Get((int)ShaderChannel4.Tangent));
				curBits.Set((int)ShaderChannel4.Tangent, false);
				return curBits.ToUInt32();
			}
		}

		private static ChannelInfo[] GetChannels(IExportContainer container, Mesh originMesh)
		{
			ref VertexData origin = ref originMesh.VertexData;
			if (ShaderChannelExtensions.ShaderChannel2018Relevant(container.Version)) // 2018.1 <= Version
			{
				return origin.Channels.Select(t => t.Convert(container)).ToArray();
			}
			else if (ShaderChannelExtensions.ShaderChannel5Relevant(container.Version)) // 5.0.0 <= Version < 2018.1
			{
				if (ShaderChannelExtensions.ShaderChannel2018Relevant(container.ExportVersion))
				{
					ChannelInfo[] channels = new ChannelInfo[14];
					channels[(int)ShaderChannel2018.Vertex] = origin.Channels[(int)ShaderChannel5.Vertex].Convert(container);
					channels[(int)ShaderChannel2018.Normal] = origin.Channels[(int)ShaderChannel5.Normal].Convert(container);
					channels[(int)ShaderChannel2018.Tangent] = origin.Channels[(int)ShaderChannel5.Tangent].Convert(container);
					channels[(int)ShaderChannel2018.Color] = origin.Channels[(int)ShaderChannel5.Color].Convert(container);
					channels[(int)ShaderChannel2018.UV0] = origin.Channels[(int)ShaderChannel5.UV0].Convert(container);
					channels[(int)ShaderChannel2018.UV1] = origin.Channels[(int)ShaderChannel5.UV1].Convert(container);
					channels[(int)ShaderChannel2018.UV2] = origin.Channels[(int)ShaderChannel5.UV2].Convert(container);
					channels[(int)ShaderChannel2018.UV3] = origin.Channels[(int)ShaderChannel5.UV3].Convert(container);
					ConvertSkinChannels(container, originMesh, channels);
					return channels;
				}
				else
				{
					return origin.Channels.ToArray();
				}
			}
			else if (VertexData.HasChannels(container.Version)) // 4.0.0 <= Version < 5.0.0
			{
				if (ShaderChannelExtensions.ShaderChannel2018Relevant(container.ExportVersion))
				{
					ChannelInfo[] channels = new ChannelInfo[14];
					channels[(int)ShaderChannel2018.Vertex] = origin.Channels[(int)ShaderChannel4.Vertex].Convert(container);
					channels[(int)ShaderChannel2018.Normal] = origin.Channels[(int)ShaderChannel4.Normal].Convert(container);
					channels[(int)ShaderChannel2018.Tangent] = origin.Channels[(int)ShaderChannel4.Tangent].Convert(container);
					channels[(int)ShaderChannel2018.Color] = origin.Channels[(int)ShaderChannel4.Color].Convert(container);
					channels[(int)ShaderChannel2018.UV0] = origin.Channels[(int)ShaderChannel4.UV0].Convert(container);
					channels[(int)ShaderChannel2018.UV1] = origin.Channels[(int)ShaderChannel4.UV1].Convert(container);
					ConvertSkinChannels(container, originMesh, channels);
					return channels;
				}
				else if (ShaderChannelExtensions.ShaderChannel5Relevant(container.ExportVersion))
				{
					ChannelInfo[] channels = new ChannelInfo[8];
					channels[(int)ShaderChannel5.Vertex] = origin.Channels[(int)ShaderChannel4.Vertex].Convert(container);
					channels[(int)ShaderChannel5.Normal] = origin.Channels[(int)ShaderChannel4.Normal].Convert(container);
					channels[(int)ShaderChannel5.Color] = origin.Channels[(int)ShaderChannel4.Color].Convert(container);
					channels[(int)ShaderChannel5.UV0] = origin.Channels[(int)ShaderChannel4.UV0].Convert(container);
					channels[(int)ShaderChannel5.UV1] = origin.Channels[(int)ShaderChannel4.UV1].Convert(container);
					channels[(int)ShaderChannel5.Tangent] = origin.Channels[(int)ShaderChannel4.Tangent].Convert(container);
					return channels;
				}
				else
				{
					return origin.Channels.ToArray();
				}
			}
			else  // Version < 4.0.0 - convert streams to channels
			{
				if (ShaderChannelExtensions.ShaderChannel2018Relevant(container.ExportVersion))
				{
					ChannelInfo[] channels = new ChannelInfo[14];
					channels[(int)ShaderChannel2018.Vertex] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Vertex);
					channels[(int)ShaderChannel2018.Normal] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Normal);
					channels[(int)ShaderChannel2018.Tangent] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Tangent);
					channels[(int)ShaderChannel2018.Color] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Color);
					channels[(int)ShaderChannel2018.UV0] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.UV0);
					channels[(int)ShaderChannel2018.UV1] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.UV1);
					ConvertSkinChannels(container, originMesh, channels);
					return channels;
				}
				else if (ShaderChannelExtensions.ShaderChannel5Relevant(container.ExportVersion))
				{
					ChannelInfo[] channels = new ChannelInfo[8];
					channels[(int)ShaderChannel5.Vertex] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Vertex);
					channels[(int)ShaderChannel5.Normal] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Normal);
					channels[(int)ShaderChannel5.Color] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Color);
					channels[(int)ShaderChannel5.UV0] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.UV0);
					channels[(int)ShaderChannel5.UV1] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.UV1);
					channels[(int)ShaderChannel5.Tangent] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Tangent);
					return channels;
				}
				else
				{
					ChannelInfo[] channels = new ChannelInfo[6];
					channels[(int)ShaderChannel4.Vertex] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Vertex);
					channels[(int)ShaderChannel4.Normal] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Normal);
					channels[(int)ShaderChannel4.Color] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Color);
					channels[(int)ShaderChannel4.UV0] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.UV0);
					channels[(int)ShaderChannel4.UV1] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.UV1);
					channels[(int)ShaderChannel4.Tangent] = StreamInfoConverter.GenerateChannelInfo(container, origin.Streams, ShaderChannel.Tangent);
					return channels;
				}
			}
		}

		private static void ConvertSkinChannels(IExportContainer container, Mesh origin, ChannelInfo[] channels)
		{
			if (origin.Skin.Length > 0)
			{
				byte skinStream = (byte)(channels.Where(t => t.IsSet).Max(t => t.Stream) + 1);
				byte offset = 0;

				VertexFormat weightVFormat = ShaderChannel.SkinWeight.GetVertexFormat(container.ExportVersion);
				byte weightFormat = weightVFormat.ToFormat(container.ExportVersion);
				byte weightDimention = BoneWeights4.Dimention;
				channels[(int)ShaderChannel2018.SkinWeight] = new ChannelInfo(skinStream, offset, weightFormat, weightDimention);
				offset += (byte)(BoneWeights4.Dimention * weightVFormat.GetSize(container.ExportVersion));

				VertexFormat indexVFormat = ShaderChannel.SkinBoneIndex.GetVertexFormat(container.ExportVersion);
				byte indexFormat = indexVFormat.ToFormat(container.ExportVersion);
				byte indexDimention = BoneWeights4.Dimention;
				channels[(int)ShaderChannel2018.SkinBoneIndex] = new ChannelInfo(skinStream, offset, indexFormat, indexDimention);
			}
		}

		private static byte[] GetData(IExportContainer container, Mesh originMesh, ref VertexData instance)
		{
			if (!originMesh.CheckAssetIntegrity())
			{
				return Array.Empty<byte>();
			}

			if (NeedCopyData(container))
			{
				return CopyChannelsData(container, originMesh, ref instance);
			}
			else if (NeedAppendSkin(container, ref instance))
			{
				return AppendSkin(originMesh);
			}
			else
			{
				byte[] data = originMesh.GetChannelsData();
				return data == originMesh.VertexData.Data ? data.ToArray() : data;
			}
		}

		private static bool NeedCopyData(IExportContainer container)
		{
			if (container.Platform == Platform.XBox360 && container.ExportPlatform != Platform.XBox360)
			{
				return true;
			}

			return false;
		}

		private static bool NeedAppendSkin(IExportContainer container, ref VertexData instance)
		{
			if (VertexData.ToSerializedVersion(container.Version) < 2)
			{
				if (VertexData.ToSerializedVersion(container.ExportVersion) >= 2)
				{
					if (instance.Channels[(int)ShaderChannel2018.SkinWeight].IsSet)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static byte[] CopyChannelsData(IExportContainer container, Mesh originMesh, ref VertexData instance)
		{
			int maxStream = instance.Channels.Max(t => t.Stream);
			int lastSize = instance.GetStreamSize(container.ExportVersion, maxStream);
			int lastOffset = instance.GetStreamOffset(container.ExportVersion, maxStream);
			byte[] buffer = new byte[lastOffset + lastSize];
			using (MemoryStream dstStream = new MemoryStream(buffer))
			{
				EndianType oendian = container.ExportPlatform == Platform.XBox360 ? EndianType.BigEndian : EndianType.LittleEndian;
				using (EndianWriter dst = new EndianWriter(dstStream, oendian))
				{
					using (MemoryStream srcStream = new MemoryStream(originMesh.GetChannelsData()))
					{
						EndianType iendian = container.Platform == Platform.XBox360 ? EndianType.BigEndian : EndianType.LittleEndian;
						using (EndianReader src = new EndianReader(srcStream, iendian))
						{
							CopyChannelsData(container, ref originMesh.VertexData, ref instance, src, dst);
							if (NeedAppendSkin(container, ref instance))
							{
								dstStream.Position = lastOffset;
								AppendSkin(originMesh.Skin, dst);
							}
						}
					}
				}
			}
			return buffer;
		}

		private static void CopyChannelsData(IExportContainer container, ref VertexData origin, ref VertexData instance, BinaryReader src, BinaryWriter dst)
		{
			for (ShaderChannel c = 0; c <= ShaderChannel.SkinBoneIndex; c++)
			{
				if (!c.HasChannel(container.Version))
				{
					continue;
				}
				ChannelInfo ochannel = origin.Channels[c.ToChannel(container.Version)];
				if (!ochannel.IsSet)
				{
					continue;
				}

				if (!c.HasChannel(container.ExportVersion))
				{
					continue;
				}
				ChannelInfo ichannel = instance.Channels[c.ToChannel(container.ExportVersion)];
				if (!ichannel.IsSet)
				{
					continue;
				}

				int vertexCount = origin.VertexCount;
				int ostride = origin.GetStreamStride(container.Version, ochannel.Stream);
				int istride = instance.GetStreamStride(container.ExportVersion, ichannel.Stream);
				int oextraStride = ostride - ochannel.GetStride(container.Version);
				int iextraStride = istride - ichannel.GetStride(container.ExportVersion);
				src.BaseStream.Position = origin.GetStreamOffset(container.Version, ochannel.Stream) + ochannel.Offset;
				dst.BaseStream.Position = instance.GetStreamOffset(container.ExportVersion, ichannel.Stream) + ichannel.Offset;
				VertexFormat format = ochannel.GetVertexFormat(container.Version);
				switch (format)
				{
					case VertexFormat.Float:
					case VertexFormat.Int:
						for (int i = 0; i < vertexCount; i++)
						{
							for (int j = 0; j < ochannel.Dimension; j++)
							{
								dst.Write(src.ReadUInt32());
							}
							src.BaseStream.Position += oextraStride;
							dst.BaseStream.Position += iextraStride;
						}
						break;
					case VertexFormat.Color:
						int size = format.GetSize(container.Version);
						if (size == 1)
						{
							for (int i = 0; i < vertexCount; i++)
							{
								for (int j = 0; j < ochannel.Dimension; j++)
								{
									dst.Write(src.ReadByte());
								}
								src.BaseStream.Position += oextraStride;
								dst.BaseStream.Position += iextraStride;
							}
						}
						else
						{
							for (int i = 0; i < vertexCount; i++)
							{
								for (int j = 0; j < ochannel.Dimension; j++)
								{
									dst.Write(src.ReadUInt32());
								}
								src.BaseStream.Position += oextraStride;
								dst.BaseStream.Position += iextraStride;
							}
						}
						break;
					case VertexFormat.Float16:
						for (int i = 0; i < vertexCount; i++)
						{
							for (int j = 0; j < ochannel.Dimension; j++)
							{
								dst.Write(src.ReadUInt16());
							}
							src.BaseStream.Position += oextraStride;
							dst.BaseStream.Position += iextraStride;
						}
						break;
					case VertexFormat.Byte:
						for (int i = 0; i < vertexCount; i++)
						{
							for (int j = 0; j < ochannel.Dimension; j++)
							{
								dst.Write(src.ReadByte());
							}
							src.BaseStream.Position += oextraStride;
							dst.BaseStream.Position += iextraStride;
						}
						break;

					default:
						throw new NotSupportedException(ochannel.Format.ToString());
				}
			}
		}

		private static byte[] AppendSkin(Mesh originMesh)
		{
			ref VertexData origin = ref originMesh.VertexData;
			byte[] odata = originMesh.GetChannelsData();
			int dataSize = odata.Length + GetSkinLength(originMesh.Skin);
			byte[] idata = new byte[dataSize];
			Buffer.BlockCopy(odata, 0, idata, 0, odata.Length);
			using (MemoryStream stream = new MemoryStream(idata, odata.Length, idata.Length - odata.Length))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					AppendSkin(originMesh.Skin, writer);
				}
			}
			return idata;
		}

		private static void AppendSkin(BoneWeights4[] skin, BinaryWriter writer)
		{
			for (int i = 0; i < skin.Length; i++)
			{
				ref BoneWeights4 weight = ref skin[i];
				writer.Write(weight.Weight0);
				writer.Write(weight.Weight1);
				writer.Write(weight.Weight2);
				writer.Write(weight.Weight3);
				writer.Write(weight.BoneIndex0);
				writer.Write(weight.BoneIndex1);
				writer.Write(weight.BoneIndex2);
				writer.Write(weight.BoneIndex3);
			}
		}

		private static int GetSkinLength(BoneWeights4[] skin)
		{
			int weightSize = BoneWeights4.Dimention * sizeof(float);
			int indexSize = BoneWeights4.Dimention * sizeof(int);
			return (weightSize + indexSize) * skin.Length;
		}
	}
}
