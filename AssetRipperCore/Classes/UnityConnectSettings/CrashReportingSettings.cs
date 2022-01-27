using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.UnityConnectSettings
{
	public sealed class CrashReportingSettings : IAssetReadable, IYAMLExportable
	{
		public CrashReportingSettings()
		{
			EventUrl = "https://perf-events.cloud.unity3d.com/api/events/crashes";
		}

		/// <summary>
		/// 2017.2 to 2018.3 exclusive
		/// </summary>
		public static bool HasNativeEventUrl(UnityVersion version) => version.IsGreaterEqual(2017, 2) && version.IsLess(2018, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasLogBufferSize(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.5.0 and greater and Not Release
		/// </summary>
		public static bool HasCaptureEditorExceptions(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 5);

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
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EventUrlName, EventUrl);
			if (HasNativeEventUrl(container.ExportVersion))
			{
				node.Add(NativeEventUrlName, GetNativeEventUrl());
			}
			node.Add(EnabledName, Enabled);
			if (HasLogBufferSize(container.ExportVersion))
			{
				node.Add(LogBufferSizeName, GetLogBufferSize(container.Version));
			}
			node.Add(CaptureEditorExceptionsName, GetCaptureEditorExceptions(container.Version, container.Flags));
			return node;
		}

		private string GetNativeEventUrl()
		{
			return NativeEventUrl ?? "https://perf-events.cloud.unity3d.com/symbolicate"; //not sure where this url came from
		}

		private uint GetLogBufferSize(UnityVersion version)
		{
			// NOTE: editor has different value than player
			//return HasLogBufferSize(version) ? LogBufferSize : 10;
			return 10;
		}
		private bool GetCaptureEditorExceptions(UnityVersion version, TransferInstructionFlags flags)
		{
			return true;
		}

		public string EventUrl { get; set; }
		public string NativeEventUrl { get; set; }
		public bool Enabled { get; set; }
		public uint LogBufferSize { get; set; }

		public const string EventUrlName = "m_EventUrl";
		public const string NativeEventUrlName = "m_NativeEventUrl";
		public const string EnabledName = "m_Enabled";
		public const string LogBufferSizeName = "m_LogBufferSize";
		public const string CaptureEditorExceptionsName = "m_CaptureEditorExceptions";
	}
}
