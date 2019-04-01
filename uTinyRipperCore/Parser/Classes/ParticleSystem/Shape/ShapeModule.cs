using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystems
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
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadMeshSpawn(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
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
		/// 5.5.0b11 to 2017.1.0b1
		/// </summary>
		public static bool IsReadMeshScale(Version version)
		{
			return version.IsGreaterEqual(5, 5, 0, VersionType.Beta, 11) && version.IsLessEqual(2017, 1, 0, VersionType.Beta, 1);
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
		/// Less than 5.5.0b11
		/// </summary>
		public static bool IsReadRandomDirection(Version version)
		{
			return version.IsLess(5, 5, 0, VersionType.Beta, 11);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadTexture(Version version)
		{
			return version.IsGreaterEqual(2018, 1);
		}
		/// <summary>
		/// 5.5.0b11 and greater
		/// </summary>
		public static bool IsReadRandomDirectionAmount(Version version)
		{
			return version.IsGreaterEqual(5, 5, 0, VersionType.Beta, 11);
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
		/// Less than 2017.1.0b2
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
			if (version.IsGreaterEqual(2018, 3))
			{
				return 6;
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Type = (ParticleSystemShapeType)reader.ReadInt32();
			if (IsReadRadiusSingle(reader.Version))
			{
				float radius = reader.ReadSingle();
				Radius = new MultiModeParameter(radius);
			}
			Angle = reader.ReadSingle();
			if (IsReadLength(reader.Version))
			{
				Length = reader.ReadSingle();
			}
			if (IsReadBoxAxes(reader.Version))
			{
				float boxX = reader.ReadSingle();
				float boxY = reader.ReadSingle();
				float boxZ = reader.ReadSingle();
				Scale = Type.IsBoxAny() ? new Vector3f(boxX, boxY, boxZ) : Vector3f.One;
			}
			if (IsReadArcSingle(reader.Version))
			{
				float arc = reader.ReadSingle();
				Arc = new MultiModeParameter(arc);
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
			PlacementMode = (PlacementMode)reader.ReadInt32();
			if (IsReadMeshMaterialIndex(reader.Version))
			{
				if (IsReadMeshMaterialIndexFirst(reader.Version))
				{
					MeshMaterialIndex = reader.ReadInt32();
					MeshNormalOffset = reader.ReadSingle();
				}
			}
			if (IsReadMeshSpawn(reader.Version))
			{
				MeshSpawn.Read(reader, false);
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
				float meshScale = reader.ReadSingle();
				Scale = new Vector3f(meshScale, meshScale, meshScale);
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
				bool randomDirection = reader.ReadBoolean();
				RandomDirectionAmount = randomDirection ? 1.0f : 0.0f;
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
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TypeName, (int)GetType(container.Version));
			node.Add(AngleName, Angle);
			node.Add(LengthName, GetExportLength(container.Version));
			node.Add(BoxThicknessName, BoxThickness.ExportYAML(container));
			node.Add(RadiusThicknessName, GetExportRadiusThickness(container.Version));
			node.Add(DonutRadiusName, GetExportDonutRadius(container.Version));
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(RotationName, Rotation.ExportYAML(container));
			node.Add(ScaleName, Scale.ExportYAML(container));
			node.Add(PlacementModeName, (int)PlacementMode);
			node.Add(MeshMaterialIndexName, MeshMaterialIndex);
			node.Add(MeshNormalOffsetName, MeshNormalOffset);
			if (IsReadMeshSpawn(container.Version))
			{
				node.Add(MeshSpawnName, MeshSpawn.ExportYAML(container));
			}
			node.Add(MeshName, Mesh.ExportYAML(container));
			node.Add(MeshRendererName, MeshRenderer.ExportYAML(container));
			node.Add(SkinnedMeshRendererName, SkinnedMeshRenderer.ExportYAML(container));
			node.Add(UseMeshMaterialIndexName, UseMeshMaterialIndex);
			node.Add(UseMeshColorsName, GetExportUseMeshColors(container.Version));
			node.Add(AlignToDirectionName, AlignToDirection);
			node.Add(RandomDirectionAmountName, RandomDirectionAmount);
			node.Add(SphericalDirectionAmountName, SphericalDirectionAmount);
			node.Add(RandomPositionAmountName, RandomPositionAmount);
			node.Add(RadiusName, Radius.ExportYAML(container));
			node.Add(ArcName, GetArc(container.Version).ExportYAML(container));
			return node;
		}

		private ParticleSystemShapeType GetType(Version version)
		{
			if (IsReadBoxThickness(version))
			{
				return Type;
			}
			switch(Type)
			{
				case ParticleSystemShapeType.SphereShell:
					return ParticleSystemShapeType.Sphere;
				case ParticleSystemShapeType.HemisphereShell:
					return ParticleSystemShapeType.Hemisphere;
				case ParticleSystemShapeType.ConeShell:
					return ParticleSystemShapeType.Cone;
				case ParticleSystemShapeType.ConeVolumeShell:
					return ParticleSystemShapeType.ConeVolume;
				case ParticleSystemShapeType.CircleEdge:
					return ParticleSystemShapeType.Circle;

				default:
					return Type;
			}
		}
		private float GetExportLength(Version version)
		{
			return IsReadLength(version) ? Length : 5.0f;
		}
		private float GetExportRadiusThickness(Version version)
		{
			if (IsReadBoxThickness(version))
			{
				return RadiusThickness;
			}

			switch(Type)
			{
				case ParticleSystemShapeType.SphereShell:
				case ParticleSystemShapeType.HemisphereShell:
				case ParticleSystemShapeType.ConeShell:
				case ParticleSystemShapeType.ConeVolumeShell:
				case ParticleSystemShapeType.CircleEdge:
					return 0.0f;

				default:
					return 1.0f;
			}
		}
		private float GetExportDonutRadius(Version version)
		{
			return IsReadBoxThickness(version) ? DonutRadius : 0.2f;
		}
		private bool GetExportUseMeshColors(Version version)
		{
			return IsReadUseMeshMaterialIndex(version) ? UseMeshColors : true;
		}
		private int GetTextureClipChannel(Version version)
		{
			return IsReadTexture(version) ? TextureClipChannel : 3;
		}
		private bool GetTextureColorAffectsParticles(Version version)
		{
			return IsReadTexture(version) ? TextureColorAffectsParticles : true;
		}
		private bool GetTextureAlphaAffectsParticles(Version version)
		{
			return IsReadTexture(version) ? TextureAlphaAffectsParticles : true;
		}
		private MultiModeParameter GetArc(Version version)
		{
			return IsReadArcSingle(version) || IsReadRadius(version) ? Arc : new MultiModeParameter(360.0f);
		}

		public ParticleSystemShapeType Type { get; private set; }
		public float Angle { get; private set; }
		public float Length { get; private set; }
		public float ArcSingle { get; private set; }
		public float RadiusThickness { get; private set; }
		public float DonutRadius { get; private set; }
		public PlacementMode PlacementMode { get; private set; }
		public int MeshMaterialIndex { get; private set; }
		public float MeshNormalOffset { get; private set; }
		public float MeshScale { get; private set; }
		public bool UseMeshMaterialIndex { get; private set; }
		public bool UseMeshColors { get; private set; }
		public bool AlignToDirection { get; private set; }
		public int TextureClipChannel { get; private set; }
		public float TextureClipThreshold { get; private set; }
		public int TextureUVChannel { get; private set; }
		public bool TextureColorAffectsParticles { get; private set; }
		public bool TextureAlphaAffectsParticles { get; private set; }
		public bool TextureBilinearFiltering { get; private set; }
		public float RandomDirectionAmount { get; private set; }
		public float SphericalDirectionAmount { get; private set; }
		public float RandomPositionAmount { get; private set; }

		public const string TypeName = "type";
		public const string AngleName = "angle";
		public const string LengthName = "length";
		public const string BoxThicknessName = "boxThickness";
		public const string RadiusThicknessName = "radiusThickness";
		public const string DonutRadiusName = "donutRadius";
		public const string PositionName = "m_Position";
		public const string RotationName = "m_Rotation";
		public const string ScaleName = "m_Scale";
		public const string PlacementModeName = "placementMode";
		public const string MeshMaterialIndexName = "m_MeshMaterialIndex";
		public const string MeshNormalOffsetName = "m_MeshNormalOffset";
		public const string MeshSpawnName = "m_MeshSpawn";
		public const string MeshName = "m_Mesh";
		public const string MeshRendererName = "m_MeshRenderer";
		public const string SkinnedMeshRendererName = "m_SkinnedMeshRenderer";
		public const string UseMeshMaterialIndexName = "m_UseMeshMaterialIndex";
		public const string UseMeshColorsName = "m_UseMeshColors";
		public const string AlignToDirectionName = "alignToDirection";
		public const string RandomDirectionAmountName = "randomDirectionAmount";
		public const string SphericalDirectionAmountName = "sphericalDirectionAmount";
		public const string RandomPositionAmountName = "randomPositionAmount";
		public const string RadiusName = "radius";
		public const string ArcName = "arc";

		public Vector3f BoxThickness;
		public Vector3f Position;
		public Vector3f Rotation;
		public Vector3f Scale;
		public MultiModeParameter MeshSpawn;
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
