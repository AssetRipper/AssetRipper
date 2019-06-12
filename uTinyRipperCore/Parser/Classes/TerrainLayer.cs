using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class TerrainLayer : NamedObject
	{
		public TerrainLayer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}
		
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

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return DiffuseTexture.FetchDependency(file, isLog, ToLogString, DiffuseTextureName);
			yield return NormalMapTexture.FetchDependency(file, isLog, ToLogString, NormalMapTextureName);
			yield return MaskMapTexture.FetchDependency(file, isLog, ToLogString, MaskMapTextureName);
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
		public override string ExportPath => Path.Combine(AssetsKeyword, ClassIDType.Terrain.ToString(), ClassID.ToString());

		public float Metallic { get; private set; }
		public float Smoothness { get; private set; }
		public float NormalScale { get; private set; }

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

		public PPtr<Texture2D> DiffuseTexture;
		public PPtr<Texture2D> NormalMapTexture;
		public PPtr<Texture2D> MaskMapTexture;
		public Vector2f TileSize;
		public Vector2f TileOffset;
		public ColorRGBAf Specular;
		public Vector4f DiffuseRemapMin;
		public Vector4f DiffuseRemapMax;
		public Vector4f MaskMapRemapMin;
		public Vector4f MaskMapRemapMax;
	}
}
