using Ookii.CommandLine;
using System.ComponentModel;

namespace AssetRipper.Tools.CabMapGenerator;

[GeneratedParser]
[ParseOptions(IsPosix = true)]
internal sealed partial class Arguments
{
	[CommandLineArgument(IsPositional = true)]
	[Description("The input files to analyze.")]
	public string[]? FilesToExport { get; set; }

	[CommandLineArgument("output", DefaultValue = null, ShortName = 'o')]
	[Description("The output file to save the information. If not specified, it will be called \"cabmap.json\".")]
	public string? OutputFile { get; set; }
}
