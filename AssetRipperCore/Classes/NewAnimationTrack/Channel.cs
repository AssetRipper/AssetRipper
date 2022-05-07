﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.NewAnimationTrack
{
	public sealed class Channel : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			ByteOffset = reader.ReadInt32();
			Curve.Read(reader);
			AttributeName = reader.ReadString();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ByteOffsetName, ByteOffset);
			node.Add(CurveName, Curve.ExportYaml(container));
			node.Add(AttributeNameName, AttributeName);
			return node;
		}

		public int ByteOffset { get; set; }
		public string AttributeName { get; set; }

		public const string ByteOffsetName = "byteOffset";
		public const string CurveName = "curve";
		public const string AttributeNameName = "attributeName";

		public AnimationCurveTpl<Float> Curve = new();
	}
}
