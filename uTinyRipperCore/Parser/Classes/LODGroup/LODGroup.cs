using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.LODGroups;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class LODGroup : Component
	{
		public LODGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadScreenRelativeTransitionHeightName(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool IsReadFadeMode(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadLastLODIsBillboard(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsAlign2(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static int GetSerializedVersion(Version version)
		{
			// FadeMode has been transfered from LOD to LODGroup
			if (version.IsGreaterEqual(5, 1))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			LocalReferencePoint.Read(reader);
			Size = reader.ReadSingle();
			if (IsReadScreenRelativeTransitionHeightName(reader.Version))
			{
				ScreenRelativeTransitionHeight = reader.ReadSingle();
			}
			if (IsReadFadeMode(reader.Version))
			{
				FadeMode = (LODFadeMode)reader.ReadInt32();
				AnimateCrossFading = reader.ReadBoolean();
			}
			if (IsReadLastLODIsBillboard(reader.Version))
			{
				LastLODIsBillboard = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			
			m_lods = reader.ReadAssetArray<LOD>();
			Enabled = reader.ReadBoolean();
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (LOD lod in LODs)
			{
				foreach (Object asset in lod.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(LocalReferencePointName, LocalReferencePoint.ExportYAML(container));
			node.Add(SizeName, Size);
			node.Add(FadeModeName, (int)GetFadeMode(container.Version));
			node.Add(AnimateCrossFadingName, AnimateCrossFading);
			node.Add(LastLODIsBillboardName, LastLODIsBillboard);
			node.Add(LODsName, LODs.ExportYAML(container));
			node.Add(EnabledName, Enabled);
			return node;
		}

		private LODFadeMode GetFadeMode(Version version)
		{
			return IsReadFadeMode(version) ? FadeMode : LODs[0].FadeMode;
		}

		public float Size { get; private set; }
		public float ScreenRelativeTransitionHeight { get; private set; }
		public LODFadeMode FadeMode { get; private set; }
		public bool AnimateCrossFading { get; private set; }
		public bool LastLODIsBillboard { get; private set; }
		public IReadOnlyList<LOD> LODs => m_lods;
		public bool Enabled { get; private set; }

		public const string LocalReferencePointName = "m_LocalReferencePoint";
		public const string SizeName = "m_Size";
		public const string ScreenRelativeTransitionHeightName = "m_ScreenRelativeTransitionHeight";
		public const string FadeModeName = "m_FadeMode";
		public const string AnimateCrossFadingName = "m_AnimateCrossFading";
		public const string LastLODIsBillboardName = "m_LastLODIsBillboard";
		public const string LODsName = "m_LODs";
		public const string EnabledName = "m_Enabled";

		public Vector3f LocalReferencePoint;

		private LOD[] m_lods;
	}
}
