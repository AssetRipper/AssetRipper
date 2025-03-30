using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Text.RegularExpressions;

namespace AssetRipper.SourceGenerated.Extensions;

public static partial class MonoScriptExtensions
{
	[GeneratedRegex(@"^(\w+)`([1-9][0-9]*)$")]
	private static partial Regex GenericRegex { get; }

	public static bool IsFullNameEqual(this IMonoScript _this, IMonoScript other)
	{
		return _this.AssemblyName == other.AssemblyName
			&& _this.Namespace == other.Namespace
			&& _this.ClassName_R == other.ClassName_R;
	}

	public static bool IsType(this IMonoScript _this, string @namespace, string name)
	{
		return _this.Namespace == @namespace && _this.ClassName_R == name;
	}

	public static bool IsGeneric(this IMonoScript script, out string genericName, out int genericCount)
	{
		return IsGeneric(script.ClassName_R.String, out genericName, out genericCount);
	}

	public static bool IsGeneric(string className, out string genericName, out int genericCount)
	{
		Match match = GenericRegex.Match(className);
		if (match.Success)
		{
			genericName = match.Groups[1].Value;
			if (int.TryParse(match.Groups[2].Value, out genericCount))
			{
				return true;
			}
		}
		genericName = className;
		genericCount = 0;
		return false;
	}

	public static string GetNonGenericClassName(this IMonoScript script)
	{
		return IsGeneric(script, out string genericName, out _) ? genericName : script.ClassName_R.String;
	}
}
