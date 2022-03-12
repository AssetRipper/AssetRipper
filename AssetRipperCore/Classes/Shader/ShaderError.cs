using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader
{
	public sealed class ShaderError : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasMessageDetails(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasFile(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.5.0f3 and greater
		/// </summary>
		public static bool IsRuntimeError(UnityVersion version) => version.IsGreaterEqual(5, 5, 0, UnityVersionType.Final, 3);

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
			if (HasMessageDetails(container.ExportVersion))
			{
				node.Add(MessageDetailsName, GetMessageDetails(container.Version));
			}

			if (HasFile(container.ExportVersion))
			{
				node.Add(FileName, GetFile(container.Version));
				node.Add(CompilerPlatformName, (int)CompilerPlatform);
			}

			node.Add(LineName, Line);
			node.Add(WarningName, Warning);
			node.Add(ProgramErrorName(container.ExportVersion), ProgramError);
			return node;
		}

		private string GetMessageDetails(UnityVersion version)
		{
			return HasMessageDetails(version) ? MessageDetails : string.Empty;
		}
		private string GetFile(UnityVersion version)
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
		public string ProgramErrorName(UnityVersion version) => IsRuntimeError(version) ? "runtimeError" : "programError";
	}
}
