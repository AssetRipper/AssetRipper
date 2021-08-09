using AssetRipper.Core.Project;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc
{
	public struct StreamingInfo : IAsset
	{
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool IsOffsetInt64(UnityVersion version) => version.IsGreaterEqual(2020);

		public StreamingInfo(bool _)
		{
			Offset = 0;
			Size = 0;
			Path = string.Empty;
		}

		public StreamingInfo(UnityVersion version)
		{
			Offset = 0;
			Size = 0;
			Path = string.Empty;
		}

		public bool CheckIntegrity(ISerializedFile file)
		{
			if (!IsSet)
			{
				return true;
			}
			return file.Collection.FindResourceFile(Path) != null;
		}

		public byte[] GetContent(ISerializedFile file)
		{
			IResourceFile res = file.Collection.FindResourceFile(Path);
			if (res == null)
			{
				return null;
			}

			byte[] data = new byte[Size];
			res.Stream.Position = Offset;
			res.Stream.ReadBuffer(data, 0, data.Length);
			return data;
		}

		public void Read(AssetReader reader)
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

		public void Write(AssetWriter writer)
		{
			writer.Write((uint)Offset);
			writer.Write(Size);
			writer.Write(Path);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(OffsetName, (uint)Offset);
			node.Add(SizeName, Size);
			node.Add(PathName, Path);
			return node;
		}

		public const string OffsetName = "offset";
		public const string SizeName = "size";
		public const string PathName = "path";

		public bool IsSet => Path.Length > 0;

		public long Offset { get; set; }
		public uint Size { get; set; }
		public string Path { get; set; }
	}
}
