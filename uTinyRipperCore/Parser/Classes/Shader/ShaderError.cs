using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderError : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasMessageDetails(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasFile(Version version) => version.IsGreaterEqual(4, 5);

		public void Read(AssetReader reader)
		{
			Message = reader.ReadString();
			if (HasMessageDetails(reader.Version))
			{
				MessageDetails = reader.ReadString();
			}
			if (HasFile(reader.Version))
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
			return HasMessageDetails(version) ? MessageDetails : string.Empty;
		}
		private string GetFile(Version version)
		{
			return HasFile(version) ? File : string.Empty;
		}

		public string Message { get; set; }
		public string MessageDetails { get; set; }
		public string File { get; set; }
		public Platform CompilerPlatform { get; set; }
		public string Line { get; set; }
		public bool Warning { get; set; }
		public bool ProgramError { get; set; }

		public const string MessageName = "message";
		public const string MessageDetailsName = "messageDetails";
		public const string FileName = "file";
		public const string CompilerPlatformName = "compilerPlatform";
		public const string LineName = "line";
		public const string WarningName = "warning";
		public const string ProgramErrorName = "programError";
	}
}
