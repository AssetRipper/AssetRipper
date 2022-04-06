using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

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

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			if (HasCutoffFrequency(container.ExportVersion))
            {
                node.Add("m_CutoffFrequency", CutoffFrequency);
            }
			node.Add("m_LowpassResonanceQ", LowpassResonanceQ);
			node.Add("lowpassLevelCustomCurve", lowpassLevelCustomCurve.ExportYAML(container));
			return node;
		}

		public float LowpassResonanceQ { get; set; }
		public float CutoffFrequency { get; set; }
		public AnimationCurveTpl<Float> lowpassLevelCustomCurve = new();
	}
}
