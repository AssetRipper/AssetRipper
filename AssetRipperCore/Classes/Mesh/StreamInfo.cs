﻿using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class StreamInfo : IStreamInfo
	{
		public uint ChannelMask { get; set; }
		public uint Offset { get; set; }
		public uint Stride { get; set; }
		public uint Align { get; set; }
		public byte DividerOp { get; set; }
		public ushort Frequency { get; set; }

		public const string ChannelMaskName = "channelMask";
		public const string OffsetName = "offset";
		public const string StrideName = "stride";
		public const string AlignName = "align";
		public const string DividerOpMaskName = "dividerOp";
		public const string FrequencyMaskName = "frequency";

		public StreamInfo() { }

		public StreamInfo(uint mask, uint offset, uint stride)
		{
			ChannelMask = mask;
			Offset = offset;
			Stride = stride;
			Align = 0;
			DividerOp = 0;
			Frequency = 0;
		}

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasAlign(UnityVersion version) => version.IsLess(4);

		public bool IsMatch(ShaderChannel4 channel)
		{
			return (ChannelMask & (1 << (int)channel)) != 0;
		}

		public void Read(AssetReader reader)
		{
			ChannelMask = reader.ReadUInt32();
			Offset = reader.ReadUInt32();

			if (HasAlign(reader.Version))
			{
				Stride = reader.ReadUInt32();
				Align = reader.ReadUInt32();
			}
			else
			{
				Stride = reader.ReadByte();
				DividerOp = reader.ReadByte();
				Frequency = reader.ReadUInt16();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(ChannelMask);
			writer.Write(Offset);

			if (HasAlign(writer.Version))
			{
				writer.Write(Stride);
				writer.Write(Align);
			}
			else
			{
				writer.Write((byte)Stride);
				writer.Write(DividerOp);
				writer.Write(Frequency);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ChannelMaskName, ChannelMask);
			node.Add(OffsetName, Offset);
			node.Add(StrideName, Stride);

			if (HasAlign(container.ExportVersion))
			{
				node.Add(AlignName, Align);
			}
			else
			{
				node.Add(DividerOpMaskName, DividerOp);
				node.Add(FrequencyMaskName, Frequency);
			}
			return node;
		}
	}
}
