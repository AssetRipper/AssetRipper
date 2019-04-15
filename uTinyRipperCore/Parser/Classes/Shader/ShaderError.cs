using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderError : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadMessageDetails(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadFile(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}

		public void Read(AssetReader reader)
		{
			Message = reader.ReadString();
			if (IsReadMessageDetails(reader.Version))
			{
				MessageDetails = reader.ReadString();
			}
			if (IsReadFile(reader.Version))
			{
				File = reader.ReadString();
				CompilerPlatform = (Platform)reader.ReadInt32();
			}
			Line = reader.ReadString();
			Warning = reader.ReadBoolean();
			ProgramError = reader.ReadBoolean();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MessageName, Message);
			node.Add(MessageDetailsName, GetMessageDetails(container.Version));
			node.Add(FileName, GetFile(container.Version));
			node.Add(CompilerPlatformName, (int)CompilerPlatform);
			node.Add(LineName, Line);
			node.Add(WarningName, Warning);
			node.Add(ProgramErrorName, ProgramError);
			return node;
		}

		private string GetMessageDetails(Version version)
		{
			return IsReadMessageDetails(version) ? MessageDetails : string.Empty;
		}
		private string GetFile(Version version)
		{
			return IsReadFile(version) ? File : string.Empty;
		}

		public string Message { get; private set; }
		public string MessageDetails { get; private set; }
		public string File { get; private set; }
		public Platform CompilerPlatform { get; private set; }
		public string Line { get; private set; }
		public bool Warning { get; private set; }
		public bool ProgramError { get; private set; }

		public const string MessageName = "message";
		public const string MessageDetailsName = "messageDetails";
		public const string FileName = "file";
		public const string CompilerPlatformName = "compilerPlatform";
		public const string LineName = "line";
		public const string WarningName = "warning";
		public const string ProgramErrorName = "programError";
	}
}
