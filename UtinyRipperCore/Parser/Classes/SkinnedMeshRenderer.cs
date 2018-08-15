using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class SkinnedMeshRenderer : Renderer
	{
		public SkinnedMeshRenderer(AssetInfo assetInfo):
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
		public static bool IsReadQuality(Version version)
		{
			return version.IsGreaterEqual(1, 5);
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(2, 6))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			if (IsReadRenderer(stream.Version))
			{
				base.Read(stream);
			}
			else
			{
				ReadBase(stream);
			}

			if (IsReadQuality(stream.Version))
			{
				if (IsReadQualityFirst(stream.Version))
				{
					Quality = stream.ReadInt32();
				}
			}
			if (IsReadUpdateWhenOffscreen(stream.Version))
			{
				UpdateWhenOffscreen = stream.ReadBoolean();
			}
			if (IsReadQuality(stream.Version))
			{
				if (!IsReadQualityFirst(stream.Version))
				{
					Quality = stream.ReadInt32();
				}
			}
			
			if(IsReadSkinNormals(stream.Version))
			{
				SkinNormals = stream.ReadBoolean();
			}
			if(IsReadSkinMotionVector(stream.Version))
			{
				SkinnedMotionVectors = stream.ReadBoolean();
			}
			if (IsAlignBools(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if(IsReadOffscreen(stream.Version))
			{
				DisableAnimationWhenOffscreen.Read(stream);
			}
			
			if (IsReadMeshFirst(stream.Version))
			{
				Mesh.Read(stream);
			}

			if (IsReadAnimation(stream.Version))
			{
				Animation.Read(stream);
			}

			m_bones = stream.ReadArray<PPtr<Transform>>();
			if (IsAlignBones(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (!IsReadMeshFirst(stream.Version))
			{
				Mesh.Read(stream);
			}

			if(IsReadBindPose(stream.Version))
			{
				m_bindPose = stream.ReadArray<Matrix4x4f>();
			}
			if (IsReadCurrentPose(stream.Version))
			{
				CurrentPose.Read(stream);
			}
			
			if(IsReadWeights(stream.Version))
			{
				m_blendShapeWeights = stream.ReadSingleArray();
			}
			if(IsReadRootBone(stream.Version))
			{
				RootBone.Read(stream);
			}
			if(IsReadAABB(stream.Version))
			{
				AABB.Read(stream);
				DirtyAABB = stream.ReadBoolean();
				if(IsAlignDirty(stream.Version))
				{
					stream.AlignStream(AlignType.Align4);
				}
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			if(IsReadOffscreen(file.Version))
			{
				yield return DisableAnimationWhenOffscreen.FetchDependency(file, isLog, ToLogString, "m_DisableAnimationWhenOffscreen");
			}
			yield return Mesh.FetchDependency(file, isLog, ToLogString, "m_Mesh");
			foreach (PPtr<Transform> ptr in Bones)
			{
				if (!ptr.IsNull)
				{
					yield return ptr.GetAsset(file);
				}
			}
			if (!RootBone.IsNull)
			{
				yield return RootBone.GetAsset(file);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Quality", Quality);
			node.Add("m_UpdateWhenOffscreen", UpdateWhenOffscreen);
			node.Add("m_skinnedMotionVectors", SkinnedMotionVectors);
			node.Add("m_Mesh", Mesh.ExportYAML(container));
			node.Add("m_Bones", Bones.ExportYAML(container));
			node.Add("m_BlendShapeWeights", IsReadWeights(container.Version) ? m_blendShapeWeights.ExportYAML() : YAMLSequenceNode.Empty);
			node.Add("m_RootBone", RootBone.ExportYAML(container));
			node.Add("m_AABB", AABB.ExportYAML(container));
			node.Add("m_DirtyAABB", DirtyAABB);
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
