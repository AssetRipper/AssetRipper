using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Font
{
	public sealed class CharacterInfo : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(1, 6))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 1.6.0 and greater
		/// </summary>
		public static bool HasIndex(UnityVersion version) => version.IsGreaterEqual(1, 6);
		/// <summary>
		/// 1.6.0 to 5.3.0 exclusive
		/// </summary>
		public static bool HasWidth(UnityVersion version) => version.IsGreaterEqual(1, 6) && version.IsLess(5, 3);
		/// <summary>
		/// 5.3.0 and greater 
		/// </summary>
		public static bool HasAdvance(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasFlipped(UnityVersion version) => version.IsGreaterEqual(4);

		public void Read(AssetReader reader)
		{
			if (HasIndex(reader.Version))
			{
				Index = reader.ReadInt32();
			}
			UV.Read(reader);
			Vert.Read(reader);

			if (HasWidth(reader.Version))
			{
				Width = reader.ReadSingle();
			}
			if (HasAdvance(reader.Version))
			{
				Advance = reader.ReadSingle();
			}
			if (HasFlipped(reader.Version))
			{
				Flipped = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(IndexName, Index);
			node.Add(UVName, UV.ExportYAML(container));
			node.Add(VertName, Vert.ExportYAML(container));
			node.Add(AdvanceName, Advance);
			node.Add(FlippedName, Flipped);
			return node;
		}

		public int Index { get; set; }
		public float Width { get; set; }
		public float Advance { get; set; }
		public bool Flipped { get; set; }

		public const string IndexName = "index";
		public const string UVName = "uv";
		public const string VertName = "vert";
		public const string AdvanceName = "advance";
		public const string FlippedName = "flipped";

		public Rectf UV = new();
		public Rectf Vert = new();
	}
}
