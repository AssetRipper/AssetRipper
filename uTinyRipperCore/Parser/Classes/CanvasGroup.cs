using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class CanvasGroup : Behaviour
	{
		public CanvasGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static bool HasBehaviour(Version version)
		{
			return version.IsGreaterEqual(4, 6, 1);
		}

		public override void Read(AssetReader reader)
		{
			if (HasBehaviour(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadComponent(reader);
			}

			Alpha = reader.ReadSingle();
			Interactable = reader.ReadBoolean();
			BlocksRaycasts = reader.ReadBoolean();
			IgnoreParentGroups = reader.ReadBoolean();
			reader.AlignStream();
			
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

		public float Alpha { get; set; }
		public bool Interactable { get; set; }
		public bool BlocksRaycasts { get; set; }
		public bool IgnoreParentGroups { get; set; }

		public const string AlphaName = "m_Alpha";
		public const string InteractableName = "m_Interactable";
		public const string BlocksRaycastsName = "m_BlocksRaycasts";
		public const string IgnoreParentGroupsName = "m_IgnoreParentGroups";
	}
}
