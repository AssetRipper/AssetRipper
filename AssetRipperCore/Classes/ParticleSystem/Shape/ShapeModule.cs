using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ParticleSystem.Shape
{
	public sealed class ShapeModule : ParticleSystemModule, IDependent
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return 6;
			}
			if (version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2))
			{
				return 5;
			}
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5))
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

		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool IsMultimodeParameter(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasLength(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// Less than 2017.1.0b2
		/// </summary>
		public static bool HasBoxAxes(UnityVersion version) => version.IsLess(2017, 1, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasArc(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasBoxThickness(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasMeshMaterialIndex(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasMeshSpawn(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasMeshRenderer(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasSprite(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 5.5.0b11 to 2017.1.0b1
		/// </summary>
		public static bool HasMeshScale(UnityVersion version)
		{
			return version.IsGreaterEqual(5, 5, 0, UnityVersionType.Beta, 11) && version.IsLessEqual(2017, 1, 0, UnityVersionType.Beta, 1);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasUseMeshMaterialIndex(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasAlignToDirection(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// Less than 5.5.0b11
		/// </summary>
		public static bool HasRandomDirection(UnityVersion version) => version.IsLess(5, 5, 0, UnityVersionType.Beta, 11);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasTexture(UnityVersion version) => version.IsGreaterEqual(2018, 1);
		/// <summary>
		/// 5.5.0b11 and greater
		/// </summary>
		public static bool HasRandomDirectionAmount(UnityVersion version) => version.IsGreaterEqual(5, 5, 0, UnityVersionType.Beta, 11);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasRandomPositionAmount(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);

		/// <summary>
		/// Less than 2017.1.0b2
		/// </summary>
		private static bool HasRadiusFirst(UnityVersion version) => version.IsLess(2017, 1, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// Less than 2017.1.0b2
		/// </summary>
		private static bool HasMeshMaterialIndexFirst(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Type = (ParticleSystemShapeType)reader.ReadInt32();
			if (!IsMultimodeParameter(reader.Version))
			{
				float radius = reader.ReadSingle();
				Radius = new MultiModeParameter(radius);
			}
			Angle = reader.ReadSingle();
			if (HasLength(reader.Version))
			{
				Length = reader.ReadSingle();
			}
			if (HasBoxAxes(reader.Version))
			{
				float boxX = reader.ReadSingle();
				float boxY = reader.ReadSingle();
				float boxZ = reader.ReadSingle();
				Scale = Type.IsBoxAny() ? new Vector3f(boxX, boxY, boxZ) : Vector3f.One;
			}
			if (IsMultimodeParameter(reader.Version))
			{
				if (HasRadiusFirst(reader.Version))
				{
					Radius.Read(reader);
					Arc.Read(reader);
				}
			}
			else
			{
				if (HasArc(reader.Version))
				{
					float arc = reader.ReadSingle();
					Arc = new MultiModeParameter(arc);
				}
			}
			if (HasBoxThickness(reader.Version))
			{
				BoxThickness.Read(reader);
				RadiusThickness = reader.ReadSingle();
				DonutRadius = reader.ReadSingle();
				Position.Read(reader);
				Rotation.Read(reader);
				Scale.Read(reader);
			}
			PlacementMode = (PlacementMode)reader.ReadInt32();
			if (HasMeshMaterialIndex(reader.Version))
			{
				if (HasMeshMaterialIndexFirst(reader.Version))
				{
					MeshMaterialIndex = reader.ReadInt32();
					MeshNormalOffset = reader.ReadSingle();
				}
			}
			if (HasMeshSpawn(reader.Version))
			{
				MeshSpawn.Read(reader, false);
			}
			Mesh.Read(reader);
			if (HasMeshRenderer(reader.Version))
			{
				MeshRenderer.Read(reader);
				SkinnedMeshRenderer.Read(reader);
			}
			if (HasSprite(reader.Version))
			{
				Sprite.Read(reader);
				SpriteRenderer.Read(reader);
			}
			if (HasMeshMaterialIndex(reader.Version))
			{
				if (!HasMeshMaterialIndexFirst(reader.Version))
				{
					MeshMaterialIndex = reader.ReadInt32();
					MeshNormalOffset = reader.ReadSingle();
				}
			}
			if (HasMeshScale(reader.Version))
			{
				float meshScale = reader.ReadSingle();
				Scale = new Vector3f(meshScale, meshScale, meshScale);
			}
			if (HasUseMeshMaterialIndex(reader.Version))
			{
				UseMeshMaterialIndex = reader.ReadBoolean();
				UseMeshColors = reader.ReadBoolean();
			}
			if (HasAlignToDirection(reader.Version))
			{
				AlignToDirection = reader.ReadBoolean();
			}
			if (HasRandomDirection(reader.Version))
			{
				bool randomDirection = reader.ReadBoolean();
				RandomDirectionAmount = randomDirection ? 1.0f : 0.0f;
			}
			reader.AlignStream();

			if (HasTexture(reader.Version))
			{
				Texture.Read(reader);
				TextureClipChannel = reader.ReadInt32();
				TextureClipThreshold = reader.ReadSingle();
				TextureUVChannel = reader.ReadInt32();
				TextureColorAffectsParticles = reader.ReadBoolean();
				TextureAlphaAffectsParticles = reader.ReadBoolean();
				TextureBilinearFiltering = reader.ReadBoolean();
				reader.AlignStream();
			}

			if (HasRandomDirectionAmount(reader.Version))
			{
				RandomDirectionAmount = reader.ReadSingle();
				SphericalDirectionAmount = reader.ReadSingle();
			}
			if (HasRandomPositionAmount(reader.Version))
			{
				RandomPositionAmount = reader.ReadSingle();
			}
			if (IsMultimodeParameter(reader.Version))
			{
				if (!HasRadiusFirst(reader.Version))
				{
					Radius.Read(reader);
					Arc.Read(reader);
				}
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Mesh, MeshName);
			yield return context.FetchDependency(MeshRenderer, MeshRendererName);
			yield return context.FetchDependency(SkinnedMeshRenderer, SkinnedMeshRendererName);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
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
			if (HasMeshSpawn(container.Version))
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

		private ParticleSystemShapeType GetType(UnityVersion version)
		{
			if (HasBoxThickness(version))
			{
				return Type;
			}
			return Type switch
			{
				ParticleSystemShapeType.SphereShell => ParticleSystemShapeType.Sphere,
				ParticleSystemShapeType.HemisphereShell => ParticleSystemShapeType.Hemisphere,
				ParticleSystemShapeType.ConeShell => ParticleSystemShapeType.Cone,
				ParticleSystemShapeType.ConeVolumeShell => ParticleSystemShapeType.ConeVolume,
				ParticleSystemShapeType.CircleEdge => ParticleSystemShapeType.Circle,
				_ => Type,
			};
		}
		private float GetExportLength(UnityVersion version)
		{
			return HasLength(version) ? Length : 5.0f;
		}
		private float GetExportRadiusThickness(UnityVersion version)
		{
			if (HasBoxThickness(version))
			{
				return RadiusThickness;
			}

			switch (Type)
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
		private float GetExportDonutRadius(UnityVersion version)
		{
			return HasBoxThickness(version) ? DonutRadius : 0.2f;
		}
		private bool GetExportUseMeshColors(UnityVersion version)
		{
			return HasUseMeshMaterialIndex(version) ? UseMeshColors : true;
		}
		private int GetTextureClipChannel(UnityVersion version)
		{
			return HasTexture(version) ? TextureClipChannel : 3;
		}
		private bool GetTextureColorAffectsParticles(UnityVersion version)
		{
			return HasTexture(version) ? TextureColorAffectsParticles : true;
		}
		private bool GetTextureAlphaAffectsParticles(UnityVersion version)
		{
			return HasTexture(version) ? TextureAlphaAffectsParticles : true;
		}
		private MultiModeParameter GetArc(UnityVersion version)
		{
			return HasArc(version) ? Arc : new MultiModeParameter(360.0f);
		}

		public ParticleSystemShapeType Type { get; set; }
		public float Angle { get; set; }
		public float Length { get; set; }
		public float ArcSingle { get; set; }
		public float RadiusThickness { get; set; }
		public float DonutRadius { get; set; }
		public PlacementMode PlacementMode { get; set; }
		public int MeshMaterialIndex { get; set; }
		public float MeshNormalOffset { get; set; }
		public float MeshScale { get; set; }
		public bool UseMeshMaterialIndex { get; set; }
		public bool UseMeshColors { get; set; }
		public bool AlignToDirection { get; set; }
		public int TextureClipChannel { get; set; }
		public float TextureClipThreshold { get; set; }
		public int TextureUVChannel { get; set; }
		public bool TextureColorAffectsParticles { get; set; }
		public bool TextureAlphaAffectsParticles { get; set; }
		public bool TextureBilinearFiltering { get; set; }
		public float RandomDirectionAmount { get; set; }
		public float SphericalDirectionAmount { get; set; }
		public float RandomPositionAmount { get; set; }

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

		public Vector3f BoxThickness = new();
		public Vector3f Position = new();
		public Vector3f Rotation = new();
		public Vector3f Scale = new();
		public MultiModeParameter MeshSpawn = new();
		public PPtr<Mesh.Mesh> Mesh = new();
		public PPtr<MeshRenderer> MeshRenderer = new();
		public PPtr<SkinnedMeshRenderer> SkinnedMeshRenderer = new();
		public PPtr<Sprite.Sprite> Sprite = new();
		public PPtr<SpriteRenderer.SpriteRenderer> SpriteRenderer = new();
		public PPtr<Texture2D.Texture2D> Texture = new();
		public MultiModeParameter Radius = new();
		public MultiModeParameter Arc = new();
	}
}
