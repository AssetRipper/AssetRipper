using Ookii.CommandLine;
using System.ComponentModel;

namespace AssetRipper.GUI.Web;

[GeneratedParser]
[ParseOptions(IsPosix = true)]
internal sealed partial class Arguments
{
	[CommandLineArgument(DefaultValue = WebApplicationLauncher.Defaults.Port)]
	[Description("If nonzero, the application will attempt to host on this port, instead of finding a random unused port.")]
	public int Port { get; set; }

	[CommandLineArgument(DefaultValue = WebApplicationLauncher.Defaults.LaunchBrowser)]
	[Description("If true, a browser window will be launched automatically. Use --launch-browser=false to set this as false.")]
	public bool LaunchBrowser { get; set; }

	[CommandLineArgument(DefaultValue = WebApplicationLauncher.Defaults.Log)]
	[Description("If true, the application will log to a file.")]
	public bool Log { get; set; }

	[CommandLineArgument(DefaultValue = WebApplicationLauncher.Defaults.LogPath)]
	[Description("The file location at which to save the log, or a sensible default if not provided.")]
	public string? LogPath { get; set; }

	[CommandLineArgument("local-web-file")]
	[Description("Files provided with this option will replace online sources.")]
	public string[]? LocalWebFiles { get; set; }
}
