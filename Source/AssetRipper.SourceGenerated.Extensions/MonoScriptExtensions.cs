using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MonoScriptExtensions
{
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
}
