using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ExternalVersionControlExtensions
{
	private const string HiddenMeta = "Hidden Meta Files";
	private const string VisibleMeta = "Visible Meta Files";

	public static string ConvertToString(this ExternalVersionControl support)
	{
		return support switch
		{
			ExternalVersionControl.AutoDetect => "Auto detect",
			ExternalVersionControl.Disabled => HiddenMeta,
			ExternalVersionControl.Generic or ExternalVersionControl.AssetServer => VisibleMeta,
			ExternalVersionControl.Subversion => "Subversion",
			ExternalVersionControl.Perforce => "Perforce",
			_ => HiddenMeta,
		};
	}

	public static string ToStringExact(this ExternalVersionControl support)
	{
		return support switch
		{
			ExternalVersionControl.AutoDetect => "Auto detect",
			ExternalVersionControl.Disabled => "Disabled",
			ExternalVersionControl.Generic => VisibleMeta,
			ExternalVersionControl.AssetServer => "Asset Server",
			ExternalVersionControl.Subversion => "Subversion",
			ExternalVersionControl.Perforce => "Perforce",
			_ => HiddenMeta,
		};
	}

	public static ExternalVersionControl FromStringExact(string str)
	{
		return str switch
		{
			"Auto detect" => ExternalVersionControl.AutoDetect,
			"Disabled" => ExternalVersionControl.Disabled,
			"Asset Server" => ExternalVersionControl.AssetServer,
			"Subversion" => ExternalVersionControl.Subversion,
			"Perforce" => ExternalVersionControl.Perforce,
			VisibleMeta => ExternalVersionControl.Generic,
			HiddenMeta => ExternalVersionControl.Disabled,
			_ => ExternalVersionControl.Disabled,
		};
	}
}
