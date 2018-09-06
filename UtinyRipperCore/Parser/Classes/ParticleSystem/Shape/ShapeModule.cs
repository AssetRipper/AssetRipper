using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class ShapeModule : ParticleSystemModule, IDependent
	{
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsReadRadiusSingle(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadLength(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// Less than 2017.1.0b2
		/// </summary>
		public static bool IsReadBoxAxes(Version version)
		{
			return version.IsLess(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.0.0 to 5.6.0
		/// </summary>
		public static bool IsReadArcSingle(Version version)
		{
			return version.IsGreaterEqual(5) && version.IsLess(5, 6);
		}
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadBoxThickness(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadMeshMaterialIndex(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadMeshRenderer(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadSprite(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 5.5.0 to 2017.1.0b1
		/// </summary>
		public static bool IsReadMeshScale(Version version)
		{
			return version.IsGreaterEqual(5, 5) && version.IsLessEqual(2017, 1, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadUseMeshMaterialIndex(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadAlignToDirection(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadRandomDirection(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadTexture(Version version)
		{
			return version.IsGreaterEqual(2018, 1);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadRandomDirectionAmount(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadRandomPositionAmount(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadRadius(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		private static bool IsReadRadiusFirst(Version version)
		{
			return version.IsLess(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// Less than 2017.1.0b2
		/// </summary>
		private static bool IsReadMeshMaterialIndexFirst(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 5;
			}
			
			if (version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2))
			{
				return 5;
			}
			if (version.IsGreaterEqual(5, 6))
			{
				return 4;
			}
			if (version.IsGreaterEqual(5, 5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4))
			{
				return 2;
			}
			return 1;			
		}

		private float GetExportLength(Version version)
		{
			return IsReadLength(version) ? Length : 5.0f;
		}
		private float GetExportRadiusThickness(Version version)
		{
			return IsReadBoxThickness(version) ? RadiusThickness : 1.0f;
		}
		private float GetExportDonutRadius(Version version)
		{
			return IsReadBoxThickness(version) ? DonutRadius : 0.2f;
		}
		private Vector3f GetExportScale(Version version)
		{
			return IsReadBoxThickness(version) ? Scale : new Vector3f(BoxX, BoxY, BoxZ);
		}
		private bool GetExportUseMeshColors(Version version)
		{
			return IsReadUseMeshMaterialIndex(version) ? UseMeshColors : true;
		}
		private MultiModeParameter GetExportRadius(Version version)
		{
			return IsReadRadius(version) ? Radius : new MultiModeParameter(RadiusSingle);
		}
		private MultiModeParameter GetExportArc(Version version)
		{
			return IsReadRadius(version) ? Arc : new MultiModeParameter(ArcSingle);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Type = (ParticleSystemShapeType)reader.ReadInt32();
			if (IsReadRadiusSingle(reader.Version))
			{
				RadiusSingle = reader.ReadSingle();
			}
			Angle = reader.ReadSingle();
			if (IsReadLength(reader.Version))
			{
				Length = reader.ReadSingle();
			}
			if (IsReadBoxAxes(reader.Version))
			{
				BoxX = reader.ReadSingle();
				BoxY = reader.ReadSingle();
				BoxZ = reader.ReadSingle();
			}
			if (IsReadArcSingle(reader.Version))
			{
				ArcSingle = reader.ReadSingle();
			}
			if (IsReadRadius(reader.Version))
			{
				if (IsReadRadiusFirst(reader.Version))
				{
					Radius.Read(reader);
					Arc.Read(reader);
				}
			}
			if (IsReadBoxThickness(reader.Version))
			{
				BoxThickness.Read(reader);
				RadiusThickness = reader.ReadSingle();
				DonutRadius = reader.ReadSingle();
				Position.Read(reader);
				Rotation.Read(reader);
				Scale.Read(reader);
			}
			PlacementMode = reader.ReadInt32();
			if (IsReadMeshMaterialIndex(reader.Version))
			{
				if (IsReadMeshMaterialIndexFirst(reader.Version))
				{
					MeshMaterialIndex = reader.ReadInt32();
					MeshNormalOffset = reader.ReadSingle();
				}
			}
			Mesh.Read(reader);
			if (IsReadMeshRenderer(reader.Version))
			{
				MeshRenderer.Read(reader);
				SkinnedMeshRenderer.Read(reader);
			}
			if(IsReadSprite(reader.Version))
			{
				Sprite.Read(reader);
				SpriteRenderer.Read(reader);
			}
			if (IsReadMeshMaterialIndex(reader.Version))
			{
				if (!IsReadMeshMaterialIndexFirst(reader.Version))
				{
					MeshMaterialIndex = reader.ReadInt32();
					MeshNormalOffset = reader.ReadSingle();
				}
			}
			if (IsReadMeshScale(reader.Version))
			{
				MeshScale = reader.ReadSingle();
			}
			if (IsReadUseMeshMaterialIndex(reader.Version))
			{
				UseMeshMaterialIndex = reader.ReadBoolean();
				UseMeshColors = reader.ReadBoolean();
			}
			if (IsReadAlignToDirection(reader.Version))
			{
				AlignToDirection = reader.ReadBoolean();
			}
			if (IsReadRandomDirection(reader.Version))
			{
				RandomDirection = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);

			if(IsReadTexture(reader.Version))
			{
				Texture.Read(reader);
				TextureClipChannel = reader.ReadInt32();
				TextureClipThreshold = reader.ReadSingle();
				TextureUVChannel = reader.ReadInt32();
				TextureColorAffectsParticles = reader.ReadBoolean();
				TextureAlphaAffectsParticles = reader.ReadBoolean();
				TextureBilinearFiltering = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadRandomDirectionAmount(reader.Version))
			{
				RandomDirectionAmount = reader.ReadSingle();
				SphericalDirectionAmount = reader.ReadSingle();
			}
			if (IsReadRandomPositionAmount(reader.Version))
			{
				RandomPositionAmount = reader.ReadSingle();
			}
			if (IsReadRadius(reader.Version))
			{
				if (!IsReadRadiusFirst(reader.Version))
				{
					Radius.Read(reader);
					Arc.Read(reader);
				}
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Mesh.FetchDependency(file, isLog, () => nameof(ShapeModule), "m_Mesh");
			yield return MeshRenderer.FetchDependency(file, isLog, () => nameof(ShapeModule), "m_MeshRenderer");
			yield return SkinnedMeshRenderer.FetchDependency(file, isLog, () => nameof(ShapeModule), "m_SkinnedMeshRenderer");
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("type", (int)Type);
			node.Add("angle", Angle);
			node.Add("length", GetExportLength(container.Version));
			node.Add("boxThickness", BoxThickness.ExportYAML(container));
			node.Add("radiusThickness", GetExportRadiusThickness(container.Version));
			node.Add("donutRadius", GetExportDonutRadius(container.Version));
			node.Add("m_Position", Position.ExportYAML(container));
			node.Add("m_Rotation", Rotation.ExportYAML(container));
			node.Add("m_Scale", GetExportScale(container.Version).ExportYAML(container));
			node.Add("placementMode", PlacementMode);
			node.Add("m_MeshMaterialIndex", MeshMaterialIndex);
			node.Add("m_MeshNormalOffset", MeshNormalOffset);
			node.Add("m_Mesh", Mesh.ExportYAML(container));
			node.Add("m_MeshRenderer", MeshRenderer.ExportYAML(container));
			node.Add("m_SkinnedMeshRenderer", SkinnedMeshRenderer.ExportYAML(container));
			node.Add("m_UseMeshMaterialIndex", UseMeshMaterialIndex);
			node.Add("m_UseMeshColors", GetExportUseMeshColors(container.Version));
			node.Add("alignToDirection", AlignToDirection);
			node.Add("randomDirectionAmount", RandomDirectionAmount);
			node.Add("sphericalDirectionAmount", SphericalDirectionAmount);
			node.Add("randomPositionAmount", RandomPositionAmount);
			node.Add("radius", GetExportRadius(container.Version).ExportYAML(container));
			node.Add("arc", GetExportArc(container.Version).ExportYAML(container));
			return node;
		}

		public ParticleSystemShapeType Type { get; private set; }
		public float RadiusSingle { get; private set; }
		public float Angle { get; private set; }
		public float Length { get; private set; }
		public float BoxX { get; private set; }
		public float BoxY { get; private set; }
		public float BoxZ { get; private set; }
		public float ArcSingle { get; private set; }
		public float RadiusThickness { get; private set; }
		public float DonutRadius { get; private set; }
		public int PlacementMode { get; private set; }
		public int MeshMaterialIndex { get; private set; }
		public float MeshNormalOffset { get; private set; }
		public float MeshScale { get; private set; }
		public bool UseMeshMaterialIndex { get; private set; }
		public bool UseMeshColors { get; private set; }
		public bool AlignToDirection { get; private set; }
		public bool RandomDirection { get; private set; }
		public int TextureClipChannel { get; private set; }
		public float TextureClipThreshold { get; private set; }
		public int TextureUVChannel { get; private set; }
		public bool TextureColorAffectsParticles { get; private set; }
		public bool TextureAlphaAffectsParticles { get; private set; }
		public bool TextureBilinearFiltering { get; private set; }
		public float RandomDirectionAmount { get; private set; }
		public float SphericalDirectionAmount { get; private set; }
		public float RandomPositionAmount { get; private set; }

		public Vector3f BoxThickness;
		public Vector3f Position;
		public Vector3f Rotation;
		public Vector3f Scale;
		public PPtr<Mesh> Mesh;
		public PPtr<MeshRenderer> MeshRenderer;
		public PPtr<SkinnedMeshRenderer> SkinnedMeshRenderer;
		public PPtr<Sprite> Sprite;
		public PPtr<SpriteRenderer> SpriteRenderer;
		public PPtr<Texture2D> Texture;
		public MultiModeParameter Radius;
		public MultiModeParameter Arc;
	}
}
