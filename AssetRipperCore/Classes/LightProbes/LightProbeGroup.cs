using AssetRipper.Core.IO.Asset;
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
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasEnabled(UnityVersion version) => version.IsGreaterEqual(4, 0, 0);

		/// <summary>
		/// 5.6.0b6 and greater
		/// </summary>
		public static bool HasDering(UnityVersion version) => version.IsGreaterEqual(2018, 3, 0, UnityVersionType.Beta, 7);

		public LightProbeGroup(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.ReadComponent(reader);

			if (HasEnabled(reader.Version))
			{
				m_Enabled = reader.ReadByte();
				reader.AlignStream();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.WriteComponent(writer);

			if (HasEnabled(writer.Version))
			{
				writer.Write(m_Enabled);
				writer.AlignStream();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRootComponent(container);
			if (HasEnabled(container.ExportVersion))
			{
				node.Add(EnabledName, m_Enabled);
			}
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
