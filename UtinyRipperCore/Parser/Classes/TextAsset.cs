using System;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Script = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);

			if(IsReadPath(stream.Version))
			{
				PathName = stream.ReadStringAligned();
			}
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writer.Write(Script);
			}
		}

		protected void ReadBase(AssetStream stream)
		{
			base.Read(stream);
		}

		protected sealed override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}
		
		public override string ExportExtension => "bytes";

		public byte[] Script { get; private set; }
		public string PathName { get; private set; } = string.Empty;
	}
}
