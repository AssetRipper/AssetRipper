using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

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

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2, 6))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasRenderer(Version version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasUpdateWhenOffscreen(Version version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 1.5.0 to 3.2.0
		/// </summary>
		public static bool HasSkinNormals(Version version) => version.IsGreaterEqual(1, 5) && version.IsLess(3, 2);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasSkinMotionVector(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		public static bool HasOffscreen(Version version) => version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		/// <summary>
		/// 1.5.0 to 2.1.0 exclusive
		/// </summary>
		public static bool HasAnimation(Version version) => version.IsGreaterEqual(1, 5) && version.IsLess(2, 1);
		/// <summary>
		/// Less 3.0.0
		/// </summary>
		public static bool HasBindPose(Version version) => version.IsLess(3);
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		public static bool HasCurrentPose(Version version) => version.IsLess(2, 1);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasWeights(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasRootBone(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasAABB(Version version) => version.IsGreaterEqual(3, 4);

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsQualityFirst(Version version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlignBools(Version version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsMeshFirst(Version version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlignBones(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		private static bool IsAlignDirty(Version version) => version.IsGreaterEqual(4, 1);

		public override void Read(AssetReader reader)
		{
			if (HasRenderer(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadBase(reader);
			}

			if (HasUpdateWhenOffscreen(reader.Version))
			{
				if (IsQualityFirst(reader.Version))
				{
					Quality = reader.ReadInt32();
				}
				UpdateWhenOffscreen = reader.ReadBoolean();
				if (!IsQualityFirst(reader.Version))
				{
					Quality = reader.ReadInt32();
				}
			}

			if (HasSkinNormals(reader.Version))
			{
				SkinNormals = reader.ReadBoolean();
			}
			if (HasSkinMotionVector(reader.Version))
			{
				SkinnedMotionVectors = reader.ReadBoolean();
			}
			if (IsAlignBools(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasOffscreen(reader.Version))
			{
				DisableAnimationWhenOffscreen.Read(reader);
			}

			if (IsMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}

			if (HasAnimation(reader.Version))
			{
				Animation.Read(reader);
			}

			Bones = reader.ReadAssetArray<PPtr<Transform>>();
			if (IsAlignBones(reader.Version))
			{
				reader.AlignStream();
			}

			if (!IsMeshFirst(reader.Version))
			{
				Mesh.Read(reader);
			}

			if (HasBindPose(reader.Version))
			{
				BindPose = reader.ReadAssetArray<Matrix4x4f>();
			}
			if (HasCurrentPose(reader.Version))
			{
				CurrentPose.Read(reader);
			}

			if (HasWeights(reader.Version))
			{
				BlendShapeWeights = reader.ReadSingleArray();
			}
			if (HasRootBone(reader.Version))
			{
				RootBone.Read(reader);
			}
			if (HasAABB(reader.Version))
			{
				AABB.Read(reader);
				DirtyAABB = reader.ReadBoolean();
				if (IsAlignDirty(reader.Version))
				{
					reader.AlignStream();
				}
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasOffscreen(context.Version))
			{
				yield return context.FetchDependency(DisableAnimationWhenOffscreen, DisableAnimationWhenOffscreenName);
			}
			yield return context.FetchDependency(Mesh, MeshName);
			foreach (PPtr<Object> asset in context.FetchDependencies(Bones, BonesName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(RootBone, RootBoneName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(QualityName, Quality);
			node.Add(UpdateWhenOffscreenName, UpdateWhenOffscreen);
			node.Add(SkinnedMotionVectorsName, SkinnedMotionVectors);
			node.Add(MeshName, Mesh.ExportYAML(container));
			node.Add(BonesName, Bones.ExportYAML(container));
			node.Add(BlendShapeWeightsName, HasWeights(container.Version) ? BlendShapeWeights.ExportYAML() : YAMLSequenceNode.Empty);
			node.Add(RootBoneName, RootBone.ExportYAML(container));
			node.Add(AABBName, AABB.ExportYAML(container));
			node.Add(DirtyAABBName, DirtyAABB);
			return node;
		}

		public int Quality { get; set; }
		public bool UpdateWhenOffscreen { get; set; }
		public bool SkinNormals { get; set; }
		public bool SkinnedMotionVectors { get; set; }
		public PPtr<Transform>[] Bones { get; set; }
		public Matrix4x4f[] BindPose { get; set; }
		public float[] BlendShapeWeights { get; set; }
		public bool DirtyAABB { get; set; }

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
	}
}
