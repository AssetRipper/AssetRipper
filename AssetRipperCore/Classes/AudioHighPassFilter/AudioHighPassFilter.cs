using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AudioHighPassFilter
{
	public sealed class AudioHighPassFilter : AudioBehaviour
	{
		public AudioHighPassFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			CutoffFrequency = reader.ReadSingle();
			HighpassResonanceQ = reader.ReadSingle();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_CutoffFrequency", CutoffFrequency);
			node.Add("m_HighpassResonanceQ", HighpassResonanceQ);
			return node;
		}

		public float CutoffFrequency { get; set; }
		public float HighpassResonanceQ { get; set; }
	}
}
