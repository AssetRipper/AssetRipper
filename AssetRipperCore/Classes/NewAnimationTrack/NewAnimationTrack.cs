using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

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

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(CurvesName, Curves.ExportYaml(container));
			node.Add(ClassIDName, (int)AnimationClassID);
			return node;
		}

		public Channel[] Curves { get; set; }
		public ClassIDType AnimationClassID { get; set; }

		public const string CurvesName = "m_Curves";
		public const string ClassIDName = "m_ClassID";
	}
}
