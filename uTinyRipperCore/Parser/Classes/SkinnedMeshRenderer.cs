using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// SkinnedMeshFilter previously
	/// </summary>
	public sealed class SkinnedMeshRenderer : Renderer
	{
		public SkinnedMeshRenderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadRenderer(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool IsReadUpdateWhenOffscreen(Version version)
		{
			return version.IsGreaterEqual(1, 5);
		}
		/// <summary>
		/// 1.5.0 to 3.2.0
		/// </summary>
		public static bool IsReadSkinNormals(Version version)
		{
			return version.IsGreaterEqual(1, 5) && version.IsLess(3, 2);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadSkinMotionVector(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		public static bool IsReadOffscreen(Version version)
		{
			return version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		}
		/// <summary>
		/// 1.5.0 to 2.1.0 exclusive
		/// </summary>
		public static bool IsReadAnimation(Version version)
		{
			return version.IsGreaterEqual(1, 5) && version.IsLess(2, 1);
		}
		/// <summary>
		/// Less 3.0.0
		/// </summary>
		public static bool IsReadBindPose(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		public static bool IsReadCurrentPose(Version version)
		{
			return version.IsLess(2, 1);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadWeights(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadRootBone(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool IsReadAABB(Version version)
		{
			return version.IsGreaterEqual(3, 4);
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsReadQualityFirst(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlignBools(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsReadMeshFirst(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlignBones(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		private static bool IsAlignDirty(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2, 6))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			if (IsReadRenderer(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadBase(reader);
			}

			if (IsReadUpdateWhenOffscreen(reader.Version))
			{
				if (IsReadQualityFirst(reader.Version))
				{
					Quality = reader.ReadInt32();
				}
				UpdateWhenOffscreen = reader.ReadBoolean();
				if (!IsReadQualityFirst(reader.Version))
				{
					Quality = reader.ReadInt32();
				}
			}

			if (IsReadSkinNormals(reader.Version))
			{
				SkinNormals = reader.ReadBoolean();
			}
			if (IsReadSkinMotionVector(reader.Version))
			{
				SkinnedMotionVectors = reader.ReadBoolean();
			}
			if (IsAlignBools(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadOffscreen(reader.Version))
			{
				DisableAnimationWhenOffscreen.Read(reader);
			}

			if (IsReadMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}

			if (IsReadAnimation(reader.Version))
			{
				Animation.Read(reader);
			}

			m_bones = reader.ReadAssetArray<PPtr<Transform>>();
			if (IsAlignBones(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (!IsReadMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}

			if (IsReadBindPose(reader.Version))
			{
				m_bindPose = reader.ReadAssetArray<Matrix4x4f>();
			}
			if (IsReadCurrentPose(reader.Version))
			{
				CurrentPose.Read(reader);
			}

			if (IsReadWeights(reader.Version))
			{
				m_blendShapeWeights = reader.ReadSingleArray();
			}
			if (IsReadRootBone(reader.Version))
			{
				RootBone.Read(reader);
			}
			if (IsReadAABB(reader.Version))
			{
				AABB.Read(reader);
				DirtyAABB = reader.ReadBoolean();
				if (IsAlignDirty(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}
			}
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach (Object asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (IsReadOffscreen(context.Version))
			{
				yield return context.FetchDependency(DisableAnimationWhenOffscreen, DisableAnimationWhenOffscreenName);
			}
			yield return context.FetchDependency(Mesh, MeshName);
			foreach (Object asset in context.FetchDependencies(Bones, BonesName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(RootBone, RootBoneName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(QualityName, Quality);
			node.Add(UpdateWhenOffscreenName, UpdateWhenOffscreen);
			node.Add(SkinnedMotionVectorsName, SkinnedMotionVectors);
			node.Add(MeshName, Mesh.ExportYAML(container));
			node.Add(BonesName, Bones.ExportYAML(container));
			node.Add(BlendShapeWeightsName, IsReadWeights(container.Version) ? m_blendShapeWeights.ExportYAML() : YAMLSequenceNode.Empty);
			node.Add(RootBoneName, RootBone.ExportYAML(container));
			node.Add(AABBName, AABB.ExportYAML(container));
			node.Add(DirtyAABBName, DirtyAABB);
			return node;
		}

		public int Quality { get; private set; }
		public bool UpdateWhenOffscreen { get; private set; }
		public bool SkinNormals { get; private set; }
		public bool SkinnedMotionVectors { get; private set; }
		public IReadOnlyList<PPtr<Transform>> Bones => m_bones;
		public IReadOnlyList<Matrix4x4f> BindPose => m_bindPose;
		public IReadOnlyList<float> BlendShapeWeights => m_blendShapeWeights;
		public bool DirtyAABB { get; private set; }

		public const string QualityName = "m_Quality";
		public const string UpdateWhenOffscreenName = "m_UpdateWhenOffscreen";
		public const string SkinnedMotionVectorsName = "m_skinnedMotionVectors";
		public const string DisableAnimationWhenOffscreenName = "m_DisableAnimationWhenOffscreen";
		public const string MeshName = "m_Mesh";
		public const string BonesName = "m_Bones";
		public const string BlendShapeWeightsName = "m_BlendShapeWeights";
		public const string RootBoneName = "m_RootBone";
		public const string AABBName = "m_AABB";
		public const string DirtyAABBName = "m_DirtyAABB";

		public PPtr<Animation> DisableAnimationWhenOffscreen;
		/// <summary>
		/// LodMesh previously
		/// </summary>
		public PPtr<Mesh> Mesh;
		public PPtr<Animation> Animation;
		public Matrix4x4f CurrentPose;
		public PPtr<Transform> RootBone;
		public AABB AABB;

		private PPtr<Transform>[] m_bones = null;
		private Matrix4x4f[] m_bindPose;
		private float[] m_blendShapeWeights = null;
	}
}
