using System.Collections.Generic;
using uTinyRipper.Classes.LODGroups;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class LODGroup : Component
	{
		public LODGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// FadeMode has been transfered from LOD to LODGroup
			if (version.IsGreaterEqual(5, 1))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasScreenRelativeTransitionHeightName(Version version) => version.IsLess(5);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasFadeMode(Version version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasLastLODIsBillboard(Version version) => version.IsGreaterEqual(2018, 3);

		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsAlign2(Version version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			LocalReferencePoint.Read(reader);
			Size = reader.ReadSingle();
			if (HasScreenRelativeTransitionHeightName(reader.Version))
			{
				ScreenRelativeTransitionHeight = reader.ReadSingle();
			}
			if (HasFadeMode(reader.Version))
			{
				FadeMode = (LODFadeMode)reader.ReadInt32();
				AnimateCrossFading = reader.ReadBoolean();
			}
			if (HasLastLODIsBillboard(reader.Version))
			{
				LastLODIsBillboard = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			LODs = reader.ReadAssetArray<LOD>();
			Enabled = reader.ReadBoolean();
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(LODs, LODsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
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
			return HasFadeMode(version) ? FadeMode : LODs[0].FadeMode;
		}

		public float Size { get; set; }
		public float ScreenRelativeTransitionHeight { get; set; }
		public LODFadeMode FadeMode { get; set; }
		public bool AnimateCrossFading { get; set; }
		public bool LastLODIsBillboard { get; set; }
		public LOD[] LODs { get; set; }
		public bool Enabled { get; set; }

		public const string LocalReferencePointName = "m_LocalReferencePoint";
		public const string SizeName = "m_Size";
		public const string ScreenRelativeTransitionHeightName = "m_ScreenRelativeTransitionHeight";
		public const string FadeModeName = "m_FadeMode";
		public const string AnimateCrossFadingName = "m_AnimateCrossFading";
		public const string LastLODIsBillboardName = "m_LastLODIsBillboard";
		public const string LODsName = "m_LODs";
		public const string EnabledName = "m_Enabled";

		public Vector3f LocalReferencePoint;
	}
}
