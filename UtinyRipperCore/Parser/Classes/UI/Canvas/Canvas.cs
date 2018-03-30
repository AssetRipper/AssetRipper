using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Canvases;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadAlpha(stream.Version))
			{
				Alpha = stream.ReadSingle();
			}
			RenderMode = (RenderMode)stream.ReadInt32();
			Camera.Read(stream);
			if (IsReadNormals(stream.Version))
			{
				Normals = stream.ReadBoolean();
				PositionUVs = stream.ReadBoolean();
			}

			if (IsReadPlaneDistance(stream.Version))
			{
				PlaneDistance = stream.ReadSingle();
			}
			PixelPerfect = stream.ReadBoolean();

			if (IsReadRecievesEvents(stream.Version))
			{
				RecievesEvents = stream.ReadBoolean();
				OverrideSorting = stream.ReadBoolean();
				OverridePixelPerfect = stream.ReadBoolean();
				if (IsReadSortingBucketNormalizedSize(stream.Version))
				{
					SortingBucketNormalizedSize = stream.ReadSingle();
				}
				if (IsReadAdditionalShaderChannelsFlag(stream.Version))
				{
					AdditionalShaderChannelsFlag = stream.ReadInt32();
				}
				stream.AlignStream(AlignType.Align4);

				SortingLayerID = stream.ReadInt32();
				SortingOrder = stream.ReadInt16();
			}
			if (IsReadTargetDisplay(stream.Version))
			{
				TargetDisplay = stream.ReadByte();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			if(!Camera.IsNull)
			{
				Camera camera = Camera.FindObject(file);
				if(camera == null)
				{
					if(isLog)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"{ToLogString()} m_Camera {Camera.ToLogString(file)} wasn't found ");
					}
				}
				else
				{
					yield return camera;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_RenderMode", (int)RenderMode);
			node.Add("m_Camera", Camera.ExportYAML(exporter));
			node.Add("m_PlaneDistance", IsReadPlaneDistance(exporter.Version) ? PlaneDistance : 100.0f);
			node.Add("m_PixelPerfect", PixelPerfect);
			node.Add("m_ReceivesEvents", IsReadRecievesEvents(exporter.Version) ? RecievesEvents : true);
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
