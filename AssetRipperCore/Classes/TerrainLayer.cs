using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public sealed class TerrainLayer : NamedObject
	{
		public TerrainLayer(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DiffuseTexture.Read(reader);
			NormalMapTexture.Read(reader);
			MaskMapTexture.Read(reader);
			TileSize.Read(reader);
			TileOffset.Read(reader);
			Specular.Read(reader);
			Metallic = reader.ReadSingle();
			Smoothness = reader.ReadSingle();
			NormalScale = reader.ReadSingle();
			DiffuseRemapMin.Read(reader);
			DiffuseRemapMax.Read(reader);
			MaskMapRemapMin.Read(reader);
			MaskMapRemapMax.Read(reader);
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(DiffuseTexture, DiffuseTextureName);
			yield return context.FetchDependency(NormalMapTexture, NormalMapTextureName);
			yield return context.FetchDependency(MaskMapTexture, MaskMapTextureName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(DiffuseTextureName, DiffuseTexture.ExportYAML(container));
			node.Add(NormalMapTextureName, NormalMapTexture.ExportYAML(container));
			node.Add(MaskMapTextureName, MaskMapTexture.ExportYAML(container));
			node.Add(TileSizeName, TileSize.ExportYAML(container));
			node.Add(TileOffsetName, TileOffset.ExportYAML(container));
			node.Add(SpecularName, Specular.ExportYAML(container));
			node.Add(MetallicName, Metallic);
			node.Add(SmoothnessName, Smoothness);
			node.Add(NormalScaleName, NormalScale);
			node.Add(DiffuseRemapMinName, DiffuseRemapMin.ExportYAML(container));
			node.Add(DiffuseRemapMaxName, DiffuseRemapMax.ExportYAML(container));
			node.Add(MaskMapRemapMinName, MaskMapRemapMin.ExportYAML(container));
			node.Add(MaskMapRemapMaxName, MaskMapRemapMax.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "terrainlayer";
		public override string ExportPath => Path.Combine(AssetsKeyword, nameof(Terrain.Terrain), nameof(TerrainLayer));

		public float Metallic { get; set; }
		public float Smoothness { get; set; }
		public float NormalScale { get; set; }

		public const string DiffuseTextureName = "m_DiffuseTexture";
		public const string NormalMapTextureName = "m_NormalMapTexture";
		public const string MaskMapTextureName = "m_MaskMapTexture";
		public const string TileSizeName = "m_TileSize";
		public const string TileOffsetName = "m_TileOffset";
		public const string SpecularName = "m_Specular";
		public const string MetallicName = "m_Metallic";
		public const string SmoothnessName = "m_Smoothness";
		public const string NormalScaleName = "m_NormalScale";
		public const string DiffuseRemapMinName = "m_DiffuseRemapMin";
		public const string DiffuseRemapMaxName = "m_DiffuseRemapMax";
		public const string MaskMapRemapMinName = "m_MaskMapRemapMin";
		public const string MaskMapRemapMaxName = "m_MaskMapRemapMax";

		public PPtr<Texture2D.Texture2D> DiffuseTexture = new();
		public PPtr<Texture2D.Texture2D> NormalMapTexture = new();
		public PPtr<Texture2D.Texture2D> MaskMapTexture = new();
		public Vector2f TileSize = new();
		public Vector2f TileOffset = new();
		public ColorRGBAf Specular = new();
		public Vector4f DiffuseRemapMin = new();
		public Vector4f DiffuseRemapMax = new();
		public Vector4f MaskMapRemapMin = new();
		public Vector4f MaskMapRemapMax = new();
	}
}
