using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class CanvasGroup : Behaviour
	{
		public CanvasGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static bool IsReadBehaviour(Version version)
		{
			return version.IsGreaterEqual(4, 6, 1);
		}

		public override void Read(AssetReader reader)
		{
			if (IsReadBehaviour(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadBase(reader);
			}

			Alpha = reader.ReadSingle();
			Interactable = reader.ReadBoolean();
			BlocksRaycasts = reader.ReadBoolean();
			IgnoreParentGroups = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(AlphaName, Alpha);
			node.Add(InteractableName, Interactable);
			node.Add(BlocksRaycastsName, BlocksRaycasts);
			node.Add(IgnoreParentGroupsName, IgnoreParentGroups);
			return node;
		}

		public float Alpha { get; private set; }
		public bool Interactable { get; private set; }
		public bool BlocksRaycasts { get; private set; }
		public bool IgnoreParentGroups { get; private set; }

		public const string AlphaName = "m_Alpha";
		public const string InteractableName = "m_Interactable";
		public const string BlocksRaycastsName = "m_BlocksRaycasts";
		public const string IgnoreParentGroupsName = "m_IgnoreParentGroups";
	}
}
