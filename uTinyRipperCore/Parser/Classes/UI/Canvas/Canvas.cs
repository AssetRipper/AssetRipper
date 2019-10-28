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

		/// <summary>
		/// Less than 4.6.0
		/// </summary>
		public static bool IsReadAlpha(Version version)
		{
			return version.IsLess(4, 6);
		}
		/// <summary>
		/// Less than 4.6.0
		/// </summary>
		public static bool IsReadNormals(Version version)
		{
			return version.IsLess(4, 6);
		}
		/// <summary>
		/// 4.6.0 and greater
		/// </summary>
		public static bool IsReadPlaneDistance(Version version)
		{
			return version.IsGreaterEqual(4, 6);
		}
		/// <summary>
		/// 4.6.0 and greater
		/// </summary>
		public static bool IsReadRecievesEvents(Version version)
		{
			return version.IsGreaterEqual(4, 6);
		}
		/// <summary>
		/// 5.3.4 and greater
		/// </summary>
		public static bool IsReadSortingBucketNormalizedSize(Version version)
		{
			return version.IsGreaterEqual(5, 3, 4);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadTargetDisplay(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadAdditionalShaderChannelsFlag(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4, 6))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadAlpha(reader.Version))
			{
				Alpha = reader.ReadSingle();
			}
			RenderMode = (RenderMode)reader.ReadInt32();
			Camera.Read(reader);
			if (IsReadNormals(reader.Version))
			{
				Normals = reader.ReadBoolean();
				PositionUVs = reader.ReadBoolean();
			}

			if (IsReadPlaneDistance(reader.Version))
			{
				PlaneDistance = reader.ReadSingle();
			}
			PixelPerfect = reader.ReadBoolean();

			if (IsReadRecievesEvents(reader.Version))
			{
				RecievesEvents = reader.ReadBoolean();
				OverrideSorting = reader.ReadBoolean();
				OverridePixelPerfect = reader.ReadBoolean();
				if (IsReadSortingBucketNormalizedSize(reader.Version))
				{
					SortingBucketNormalizedSize = reader.ReadSingle();
				}
				if (IsReadAdditionalShaderChannelsFlag(reader.Version))
				{
					AdditionalShaderChannelsFlag = reader.ReadInt32();
				}
				reader.AlignStream(AlignType.Align4);

				SortingLayerID = reader.ReadInt32();
				SortingOrder = reader.ReadInt16();
			}
			if (IsReadTargetDisplay(reader.Version))
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
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(RenderModeName, (int)RenderMode);
			node.Add(CameraName, Camera.ExportYAML(container));
			node.Add(PlaneDistanceName, IsReadPlaneDistance(container.Version) ? PlaneDistance : 100.0f);
			node.Add(PixelPerfectName, PixelPerfect);
			node.Add(ReceivesEventsName, IsReadRecievesEvents(container.Version) ? RecievesEvents : true);
			node.Add(OverrideSortingName, OverrideSorting);
			node.Add(OverridePixelPerfectName, OverridePixelPerfect);
			node.Add(SortingBucketNormalizedSizeName, SortingBucketNormalizedSize);
			node.Add(AdditionalShaderChannelsFlagName, AdditionalShaderChannelsFlag);
			node.Add(SortingLayerIDName, SortingLayerID);
			node.Add(SortingOrderName, SortingOrder);
			node.Add(TargetDisplayName, TargetDisplay);
			return node;
		}

		public float Alpha { get; private set; }
		public RenderMode RenderMode { get; private set; }
		public bool Normals { get; private set; }
		public bool PositionUVs { get; private set; }
		public float PlaneDistance { get; private set; }
		public bool PixelPerfect { get; private set; }
		public bool RecievesEvents { get; private set; }
		public bool OverrideSorting { get; private set; }
		public bool OverridePixelPerfect { get; private set; }
		public float SortingBucketNormalizedSize { get; private set; }
		public int AdditionalShaderChannelsFlag { get; private set; }
		public int SortingLayerID { get; private set; }
		public short SortingOrder { get; private set; }
		public byte TargetDisplay { get; private set; }

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
