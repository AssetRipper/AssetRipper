using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

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
		/// 2017.2 to 2018.3 exclusive
		/// </summary>
		public static bool IsReadNativeEventUrl(Version version)
		{
			return version.IsGreaterEqual(2017, 2) && version.IsLess(2018, 3);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadLogBufferSize(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
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
			if (IsReadLogBufferSize(reader.Version))
			{
				LogBufferSize = reader.ReadUInt32();
			}
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
			node.Add(EventUrlName, EventUrl);
			if (IsReadNativeEventUrl(container.ExportVersion))
			{
				node.Add(NativeEventUrlName, GetNativeEventUrl(container.Version));
			}
			node.Add(EnabledName, Enabled);
			if (IsReadLogBufferSize(container.ExportVersion))
			{
				node.Add(LogBufferSizeName, GetLogBufferSize(container.Version));
			}
			node.Add(CaptureEditorExceptionsName, GetCaptureEditorExceptions(container.Version, container.Flags));
			return node;
		}

		private string GetNativeEventUrl(Version version)
		{
			return IsReadNativeEventUrl(version) ? NativeEventUrl : "https://perf-events.cloud.unity3d.com/symbolicate";
		}
		private uint GetLogBufferSize(Version version)
		{
			return IsReadLogBufferSize(version) ? LogBufferSize : 10;
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
		public uint LogBufferSize { get; private set; }
#if UNIVERSAL
		public bool CaptureEditorExceptions { get; private set; }
#endif

		public const string EventUrlName = "m_EventUrl";
		public const string NativeEventUrlName = "m_NativeEventUrl";
		public const string EnabledName = "m_Enabled";
		public const string LogBufferSizeName = "m_LogBufferSize";
		public const string CaptureEditorExceptionsName = "m_CaptureEditorExceptions";
	}
}
