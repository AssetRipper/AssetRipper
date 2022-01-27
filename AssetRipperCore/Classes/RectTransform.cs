using AssetRipper.Core.Converters;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public sealed class RectTransform : Transform
	{
		public RectTransform(LayoutInfo layout) : base(layout) { }

		public RectTransform(AssetInfo assetInfo) : base(assetInfo) { }

		public override IUnityObjectBase ConvertLegacy(IExportContainer container)
		{
			return RectTransformConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			AnchorMin.Read(reader);
			AnchorMax.Read(reader);
			AnchorPosition.Read(reader);
			SizeDelta.Read(reader);
			Pivot.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(AnchorMinName, AnchorMin.ExportYAML(container));
			node.Add(AnchorMaxName, AnchorMax.ExportYAML(container));
			node.Add(AnchoredPositionName, AnchorPosition.ExportYAML(container));
			node.Add(SizeDeltaName, SizeDelta.ExportYAML(container));
			node.Add(PivotName, Pivot.ExportYAML(container));
			return node;
		}

		public Vector2f AnchorMin = new();
		public Vector2f AnchorMax = new();
		public Vector2f AnchorPosition = new();
		public Vector2f SizeDelta = new();
		public Vector2f Pivot = new();

		public const string AnchorMinName = "m_AnchorMin";
		public const string AnchorMaxName = "m_AnchorMax";
		public const string AnchoredPositionName = "m_AnchoredPosition";
		public const string SizeDeltaName = "m_SizeDelta";
		public const string PivotName = "m_Pivot";
	}
}
