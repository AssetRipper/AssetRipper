using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.UnityConnectSettingss
{
	public struct CrashReportingSettings : IAssetReadable, IYAMLExportable
	{
		public CrashReportingSettings(bool _):
			this()
		{
			EventUrl = "https://perf-events.cloud.unity3d.com/api/events/crashes";
		}

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadNativeEventUrl(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadCaptureEditorExceptions(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 5);
		}

		public void Read(AssetReader reader)
		{
			EventUrl = reader.ReadString();
			if(IsReadNativeEventUrl(reader.Version))
			{
				NativeEventUrl = reader.ReadString();
			}
			Enabled = reader.ReadBoolean();
#if UNIVERSAL
			if (IsReadCaptureEditorExceptions(reader.Version, reader.Flags))
			{
				CaptureEditorExceptions = reader.ReadBoolean();
			}
#endif
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_EventUrl", EventUrl);
			node.Add("m_NativeEventUrl", GetNativeEventUrl(container.Version));
			node.Add("m_Enabled", Enabled);
			node.Add("m_CaptureEditorExceptions", GetCaptureEditorExceptions(container.Version, container.Flags));
			return node;
		}

		private string GetNativeEventUrl(Version version)
		{
			return IsReadNativeEventUrl(version) ? NativeEventUrl : "https://perf-events.cloud.unity3d.com/symbolicate";
		}
		private bool GetCaptureEditorExceptions(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadCaptureEditorExceptions(version, flags))
			{
				return CaptureEditorExceptions;
			}
#endif
			return true;
		}

		public string EventUrl { get; private set; }
		public string NativeEventUrl { get; private set; }
		public bool Enabled { get; private set; }
#if UNIVERSAL
		public bool CaptureEditorExceptions { get; private set; }
#endif
	}
}
