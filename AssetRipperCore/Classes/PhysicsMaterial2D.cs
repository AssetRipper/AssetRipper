﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes
{
	public sealed class PhysicsMaterial2D : NamedObject
	{
		public PhysicsMaterial2D(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Friction = reader.ReadSingle();
			Bounciness = reader.ReadSingle();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(FrictionName, Friction);
			node.Add(BouncinessName, Bounciness);
			return node;
		}

		public float Friction { get; set; }
		public float Bounciness { get; set; }

		public const string FrictionName = "friction";
		public const string BouncinessName = "bounciness";
	}
}
