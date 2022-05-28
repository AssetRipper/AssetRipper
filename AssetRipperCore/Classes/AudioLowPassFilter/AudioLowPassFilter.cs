using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AudioLowPassFilter
{
	public sealed class AudioLowPassFilter : AudioBehaviour
	{
		public AudioLowPassFilter(AssetInfo assetInfo) : base(assetInfo) { }
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasCutoffFrequency(reader.Version))
			{
				CutoffFrequency = reader.ReadSingle();
			}
			LowpassResonanceQ = reader.ReadSingle();
			lowpassLevelCustomCurve.Read(reader);
		}

		public static bool HasCutoffFrequency(UnityVersion version) => version.IsLess(5, 2);

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			if (HasCutoffFrequency(container.ExportVersion))
            {
                node.Add("m_CutoffFrequency", CutoffFrequency);
            }
			node.Add("m_LowpassResonanceQ", LowpassResonanceQ);
			node.Add("lowpassLevelCustomCurve", lowpassLevelCustomCurve.ExportYaml(container));
			return node;
		}

		public float LowpassResonanceQ { get; set; }
		public float CutoffFrequency { get; set; }
		public AnimationCurveTpl<Float> lowpassLevelCustomCurve = new();
	}
}
