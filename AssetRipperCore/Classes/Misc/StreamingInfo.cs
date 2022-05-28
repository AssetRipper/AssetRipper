using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class StreamingInfo : UnityAssetBase, IStreamingInfo
	{
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool IsOffsetInt64(UnityVersion version) => version.IsGreaterEqual(2020);

		public override void Read(AssetReader reader)
		{
			if (IsOffsetInt64(reader.Version))
				Offset = reader.ReadInt64();
			else
				Offset = reader.ReadUInt32();
			Size = reader.ReadUInt32();
			Path = reader.ReadString();
		}

		/// <summary>
		/// Exclusively for AudioClip in Unity versions less than 5
		/// </summary>
		public void Read(AssetReader reader, string path)
		{
			Size = reader.ReadUInt32();
			Offset = reader.ReadUInt32();
			Path = path;
		}

		public override void Write(AssetWriter writer)
		{
			writer.Write((uint)Offset);
			writer.Write(Size);
			writer.Write(Path);
		}

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(OffsetName, (uint)Offset);
			node.Add(SizeName, Size);
			node.Add(PathName, Path);
			return node;
		}

		public const string OffsetName = "offset";
		public const string SizeName = "size";
		public const string PathName = "path";

		public long Offset { get; set; }
		public uint Size { get; set; }
		public string Path { get; set; } = string.Empty;
	}
}
