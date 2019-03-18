using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Canvases;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

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

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Camera.FetchDependency(file, isLog, ToLogString, "m_Camera");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_RenderMode", (int)RenderMode);
			node.Add("m_Camera", Camera.ExportYAML(container));
			node.Add("m_PlaneDistance", IsReadPlaneDistance(container.Version) ? PlaneDistance : 100.0f);
			node.Add("m_PixelPerfect", PixelPerfect);
			node.Add("m_ReceivesEvents", IsReadRecievesEvents(container.Version) ? RecievesEvents : true);
			node.Add("m_OverrideSorting", OverrideSorting);
			node.Add("m_OverridePixelPerfect", OverridePixelPerfect);
			node.Add("m_SortingBucketNormalizedSize", SortingBucketNormalizedSize);
			node.Add("m_AdditionalShaderChannelsFlag", AdditionalShaderChannelsFlag);
			node.Add("m_SortingLayerID", SortingLayerID);
			node.Add("m_SortingOrder", SortingOrder);
			node.Add("m_TargetDisplay", TargetDisplay);
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

		public PPtr<Camera> Camera;
	}
}
