using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AudioDistortionFilter
{
	public sealed class AudioDistortionFilter : AudioBehaviour
	{
		public AudioDistortionFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DistortionLevel = reader.ReadSingle();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_DistortionLevel", DistortionLevel);
			return node;
		}

		public float DistortionLevel { get; set; }
	}
}
