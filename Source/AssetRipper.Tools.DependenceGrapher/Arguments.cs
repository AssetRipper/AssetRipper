using Ookii.CommandLine;
using System.ComponentModel;

namespace AssetRipper.Tools.DependenceGrapher;

[GeneratedParser]
[ParseOptions(IsPosix = true)]
internal sealed partial class Arguments
{
	/// <summary>
	/// This is chosen as the default because class ID numbers are always non-negative.
	/// </summary>
	private const int DefaultClassID = -1;
	/// <summary>
	/// This is chosen as the default because Unity treats a zero path id as being a null pointer.
	/// </summary>
	private const long DefaultPathID = 0;

	[CommandLineArgument(IsPositional = true)]
	[Description("The input files to analyze.")]
	public string[]? FilesToExport { get; set; }

	[CommandLineArgument("output", DefaultValue = null, ShortName = 'o')]
	[Description("The output file to save the information. If not specified, it will be called \"output.txt\".")]
	public string? OutputFile { get; set; }

	[CommandLineArgument("cab-map", DefaultValue = null)]
	[Description("If provided, a cab map json file will be used to list bundle names for referenced files.")]
	public string? CabMapPath { get; set; }

	[CommandLineArgument("name", DefaultValue = null)]
	[Description("If used, only assets with this name will be analyzed for external references.")]
	public string? Name { get; set; }

	[CommandLineArgument("class-name", DefaultValue = null)]
	[Description("If used, only assets with this class name will be analyzed for external references.")]
	public string? ClassName { get; set; }

	[CommandLineArgument("class-id", DefaultValue = DefaultClassID)]
	[Description("If used, only assets with this class ID will be analyzed for external references.")]
	public int ClassID { get; set; }

	[CommandLineArgument("path-id", DefaultValue = DefaultPathID)]
	[Description("If used, only assets with this PathID will be analyzed for external references.")]
	public long PathID { get; set; }

	[CommandLineArgument("verbose", DefaultValue = false, ShortName = 'v')]
	[Description("If true, additional information will be outputted about referencing assets.")]
	public bool Verbose { get; set; }
}
