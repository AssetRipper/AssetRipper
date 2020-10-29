using System.Collections.Generic;
using uTinyRipper.Classes.Canvases;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class Canvas : Behaviour
	{
		public Canvas(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4, 6))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 4.6.0
		/// </summary>
		public static bool HasAlpha(Version version) => version.IsLess(4, 6);
		/// <summary>
		/// Less than 4.6.0
		/// </summary>
		public static bool HasNormals(Version version) => version.IsLess(4, 6);
		/// <summary>
		/// 4.6.0 and greater
		/// </summary>
		public static bool HasPlaneDistance(Version version) => version.IsGreaterEqual(4, 6);
		/// <summary>
		/// 4.6.0 and greater
		/// </summary>
		public static bool HasRecievesEvents(Version version) => version.IsGreaterEqual(4, 6);
		/// <summary>
		/// 5.3.4 and greater
		/// </summary>
		public static bool HasSortingBucketNormalizedSize(Version version) => version.IsGreaterEqual(5, 3, 4);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasTargetDisplay(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasAdditionalShaderChannelsFlag(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasAlpha(reader.Version))
			{
				Alpha = reader.ReadSingle();
			}
			RenderMode = (RenderMode)reader.ReadInt32();
			Camera.Read(reader);
			if (HasNormals(reader.Version))
			{
				Normals = reader.ReadBoolean();
				PositionUVs = reader.ReadBoolean();
			}

			if (HasPlaneDistance(reader.Version))
			{
				PlaneDistance = reader.ReadSingle();
			}
			PixelPerfect = reader.ReadBoolean();

			if (HasRecievesEvents(reader.Version))
			{
				RecievesEvents = reader.ReadBoolean();
				OverrideSorting = reader.ReadBoolean();
				OverridePixelPerfect = reader.ReadBoolean();
				if (HasSortingBucketNormalizedSize(reader.Version))
				{
					SortingBucketNormalizedSize = reader.ReadSingle();
				}
				if (HasAdditionalShaderChannelsFlag(reader.Version))
				{
					AdditionalShaderChannelsFlag = reader.ReadInt32();
				}
				reader.AlignStream();

				SortingLayerID = reader.ReadInt32();
				SortingOrder = reader.ReadInt16();
			}
			if (HasTargetDisplay(reader.Version))
			{
				TargetDisplay = reader.ReadByte();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			yield return context.FetchDependency(Camera, CameraName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(RenderModeName, (int)RenderMode);
			node.Add(CameraName, Camera.ExportYAML(container));
			node.Add(PlaneDistanceName, HasPlaneDistance(container.Version) ? PlaneDistance : 100.0f);
			node.Add(PixelPerfectName, PixelPerfect);
			node.Add(ReceivesEventsName, HasRecievesEvents(container.Version) ? RecievesEvents : true);
			node.Add(OverrideSortingName, OverrideSorting);
			node.Add(OverridePixelPerfectName, OverridePixelPerfect);
			node.Add(SortingBucketNormalizedSizeName, SortingBucketNormalizedSize);
			node.Add(AdditionalShaderChannelsFlagName, AdditionalShaderChannelsFlag);
			node.Add(SortingLayerIDName, SortingLayerID);
			node.Add(SortingOrderName, SortingOrder);
			node.Add(TargetDisplayName, TargetDisplay);
			return node;
		}

		public float Alpha { get; set; }
		public RenderMode RenderMode { get; set; }
		public bool Normals { get; set; }
		public bool PositionUVs { get; set; }
		public float PlaneDistance { get; set; }
		public bool PixelPerfect { get; set; }
		public bool RecievesEvents { get; set; }
		public bool OverrideSorting { get; set; }
		public bool OverridePixelPerfect { get; set; }
		public float SortingBucketNormalizedSize { get; set; }
		public int AdditionalShaderChannelsFlag { get; set; }
		public int SortingLayerID { get; set; }
		public short SortingOrder { get; set; }
		public byte TargetDisplay { get; set; }

		public const string RenderModeName = "m_RenderMode";
		public const string CameraName = "m_Camera";
		public const string PlaneDistanceName = "m_PlaneDistance";
		public const string PixelPerfectName = "m_PixelPerfect";
		public const string ReceivesEventsName = "m_ReceivesEvents";
		public const string OverrideSortingName = "m_OverrideSorting";
		public const string OverridePixelPerfectName = "m_OverridePixelPerfect";
		public const string SortingBucketNormalizedSizeName = "m_SortingBucketNormalizedSize";
		public const string AdditionalShaderChannelsFlagName = "m_AdditionalShaderChannelsFlag";
		public const string SortingLayerIDName = "m_SortingLayerID";
		public const string SortingOrderName = "m_SortingOrder";
		public const string TargetDisplayName = "m_TargetDisplay";

		public PPtr<Camera> Camera;
	}
}
