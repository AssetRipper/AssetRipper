using uTinyRipper.Converters;
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
		public static bool HasNativeEventUrl(Version version) => version.IsGreaterEqual(2017, 2) && version.IsLess(2018, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasLogBufferSize(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.5.0 and greater and Not Release
		/// </summary>
		public static bool HasCaptureEditorExceptions(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 5);

		public void Read(AssetReader reader)
		{
			EventUrl = reader.ReadString();
			if (HasNativeEventUrl(reader.Version))
			{
				NativeEventUrl = reader.ReadString();
			}
			Enabled = reader.ReadBoolean();
			if (HasLogBufferSize(reader.Version))
			{
				LogBufferSize = reader.ReadUInt32();
			}
#if UNIVERSAL
			if (HasCaptureEditorExceptions(reader.Version, reader.Flags))
			{
				CaptureEditorExceptions = reader.ReadBoolean();
			}
#endif
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EventUrlName, EventUrl);
			if (HasNativeEventUrl(container.ExportVersion))
			{
				node.Add(NativeEventUrlName, GetNativeEventUrl(container.Version));
			}
			node.Add(EnabledName, Enabled);
			if (HasLogBufferSize(container.ExportVersion))
			{
				node.Add(LogBufferSizeName, GetLogBufferSize(container.Version));
			}
			node.Add(CaptureEditorExceptionsName, GetCaptureEditorExceptions(container.Version, container.Flags));
			return node;
		}

		private string GetNativeEventUrl(Version version)
		{
			return HasNativeEventUrl(version) ? NativeEventUrl : "https://perf-events.cloud.unity3d.com/symbolicate";
		}
		private uint GetLogBufferSize(Version version)
		{
			// NOTE: editor has different value than player
			//return HasLogBufferSize(version) ? LogBufferSize : 10;
			return 10;
		}
		private bool GetCaptureEditorExceptions(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasCaptureEditorExceptions(version, flags))
			{
				return CaptureEditorExceptions;
			}
#endif
			return true;
		}

		public string EventUrl { get; set; }
		public string NativeEventUrl { get; set; }
		public bool Enabled { get; set; }
		public uint LogBufferSize { get; set; }
#if UNIVERSAL
		public bool CaptureEditorExceptions { get; set; }
#endif

		public const string EventUrlName = "m_EventUrl";
		public const string NativeEventUrlName = "m_NativeEventUrl";
		public const string EnabledName = "m_Enabled";
		public const string LogBufferSizeName = "m_LogBufferSize";
		public const string CaptureEditorExceptionsName = "m_CaptureEditorExceptions";
	}
}
