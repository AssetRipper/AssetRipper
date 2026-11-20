using AssetRipper.Import;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace AssetRipper.GUI.Web.MCP;

[McpServerToolType]
public sealed class BuildTools
{
	[McpServerTool(ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false, Title = "Get Version")]
	[Description("Returns the version of this AssetRipper build.")]
	public static string GetVersion() => AssetRipperRuntimeInformation.Build.Version ?? "Unknown";

	[McpServerTool(ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false, Title = "Is Premium Edition")]
	[Description("Returns whether this is a premium edition build of AssetRipper.")]
	public static bool IsPremiumEdition() => GameFileLoader.Premium;
}
