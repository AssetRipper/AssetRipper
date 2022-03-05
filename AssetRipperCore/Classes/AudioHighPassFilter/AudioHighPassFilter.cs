using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AudioHighPassFilter
{
	public sealed class AudioHighPassFilter : AudioBehaviour
	{
		public AudioHighPassFilter(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			cutoffFrequency = reader.ReadSingle();
			highpassResonanceQ = reader.ReadSingle();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("cutoffFrequency", cutoffFrequency);
			node.Add("highpassResonanceQ", highpassResonanceQ);
			return node;
		}

		public float cutoffFrequency { get; set; }
		public float highpassResonanceQ { get; set; }
	}
}
