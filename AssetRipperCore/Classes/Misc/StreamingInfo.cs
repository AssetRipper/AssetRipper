using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Misc
{
	public struct StreamingInfo : IStreamingInfo
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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield break;
		}

		public List<TypeTreeNode> MakeReleaseTypeTreeNodes(int depth, int startingIndex)
		{
			throw new System.NotSupportedException();
		}

		public List<TypeTreeNode> MakeEditorTypeTreeNodes(int depth, int startingIndex)
		{
			throw new System.NotSupportedException();
		}

		public const string OffsetName = "offset";
		public const string SizeName = "size";
		public const string PathName = "path";

		public long Offset { get; set; }
		public uint Size { get; set; }
		public string Path { get; set; }
		public UnityVersion AssetUnityVersion { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }
		public EndianType EndianType { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }
		public TransferInstructionFlags TransferInstructionFlags { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }
	}
}
