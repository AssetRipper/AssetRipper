using System;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
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

			if(IsReadPath(reader.Version))
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

		protected sealed override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ScriptName, Script.ExportYAML());
			return node;
		}

#warning TODO:
		public override string ExportExtension => "bytes";

		public byte[] Script { get; private set; }
		public string PathName { get; private set; } = string.Empty;

		public const string ScriptName = "m_Script";
	}
}
