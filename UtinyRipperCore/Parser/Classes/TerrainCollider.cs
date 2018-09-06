using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class TerrainCollider : Collider
	{
		public TerrainCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			TerrainData.Read(reader);
			EnableTreeColliders = reader.ReadBoolean();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return TerrainData.FetchDependency(file, isLog, ToLogString, "m_TerrainData");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_TerrainData", TerrainData.ExportYAML(container));
			node.Add("m_EnableTreeColliders", EnableTreeColliders);
			return node;
		}

		/// <summary>
		/// CreateTreeColliders previously
		/// </summary>
		public bool EnableTreeColliders { get; private set; }

		public PPtr<TerrainData> TerrainData;

		protected override bool IsReadMaterial => true;
		protected override bool IsReadIsTrigger => false;
	}
}
