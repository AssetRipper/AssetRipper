using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.NewAnimationTrack
{
	public sealed class NewAnimationTrack : BaseAnimationTrack
	{
		public NewAnimationTrack(AssetInfo assetInfo) : base(assetInfo) { }

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
