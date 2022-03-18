using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.LightProbes
{
	public sealed class LightProbeGroup : Behaviour
	{
		/// <summary>
		/// 5.6.0b6 and greater
		/// </summary>
		public static bool HasDering(UnityVersion version) => version.IsGreaterEqual(2018, 3, 0, UnityVersionType.Beta, 7);

		public LightProbeGroup(AssetInfo assetInfo) : base(assetInfo) { }

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			//Editor-Only
			node.Add("m_SourcePositions", Array.Empty<Vector3f>().ExportYAML(container));
			if (HasDering(container.ExportVersion))
			{
				node.Add("m_Dering", default(bool));
			}
			return node;
		}
	}
}
