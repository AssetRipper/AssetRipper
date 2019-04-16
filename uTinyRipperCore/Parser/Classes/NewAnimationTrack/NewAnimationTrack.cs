using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.NewAnimationTracks;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class NewAnimationTrack : BaseAnimationTrack
	{
		public NewAnimationTrack(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_curves = reader.ReadAssetArray<Channel>();
			AnimationClassID = (ClassIDType)reader.ReadInt32();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(CurvesName, Curves.ExportYAML(container));
			node.Add(ClassIDName, (int)AnimationClassID);
			return node;
		}

		public IReadOnlyList<Channel> Curves => m_curves;
		public ClassIDType AnimationClassID { get; private set; }

		public const string CurvesName = "m_Curves";
		public const string ClassIDName = "m_ClassID";

		private Channel[] m_curves;
	}
}
