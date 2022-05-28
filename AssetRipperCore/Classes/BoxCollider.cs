using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes
{
	public sealed class BoxCollider : Collider
	{
		public BoxCollider(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// min version is 2nd
			return 2;
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			Size.Read(reader);
			Center.Read(reader);
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(SizeName, Size.ExportYaml(container));
			node.Add(CenterName, Center.ExportYaml(container));
			return node;
		}

		public const string SizeName = "m_Size";
		public const string CenterName = "m_Center";

		public Vector3f Size = new();
		public Vector3f Center = new();

		protected override bool IncludesIsTrigger => true;
		protected override bool IncludesMaterial => true;
	}
}
