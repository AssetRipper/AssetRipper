using uTinyRipper.Classes.NewAnimationTracks;
using uTinyRipper.Converters;
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

			Curves = reader.ReadAssetArray<Channel>();
			AnimationClassID = (ClassIDType)reader.ReadInt32();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(CurvesName, Curves.ExportYAML(container));
			node.Add(ClassIDName, (int)AnimationClassID);
			return node;
		}

		public Channel[] Curves { get; set; }
		public ClassIDType AnimationClassID { get; set; }

		public const string CurvesName = "m_Curves";
		public const string ClassIDName = "m_ClassID";
	}
}
