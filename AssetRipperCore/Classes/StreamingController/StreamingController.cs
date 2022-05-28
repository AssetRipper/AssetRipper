using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.StreamingController
{
	public sealed class StreamingController : Behaviour
	{
		public StreamingController(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			StreamingMipmapBias = reader.ReadSingle();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_StreamingMipmapBias", StreamingMipmapBias);
			return node;
		}

		public float StreamingMipmapBias { get; set; }
	}
}
