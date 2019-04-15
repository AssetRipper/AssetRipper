using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// Script previously
	/// </summary>
	public class TextAsset : NamedObject
	{
		public TextAsset(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 2017.1
		/// </summary>
		public static bool IsReadPath(Version version)
		{
			return version.IsLess(2017);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Script = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);

			if (IsReadPath(reader.Version))
			{
				PathName = reader.ReadString();
			}
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writer.Write(Script);
			}
		}

		protected void ReadBase(AssetReader reader)
		{
			base.Read(reader);
		}

		protected YAMLMappingNode ExportBaseYAMLRoot(IExportContainer container)
		{
			return base.ExportYAMLRoot(container);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ScriptName, Script.ExportYAML());
			return node;
		}

		public byte[] Script { get; protected set; }
		public string PathName { get; protected set; } = string.Empty;

		public const string ScriptName = "m_Script";
	}
}
