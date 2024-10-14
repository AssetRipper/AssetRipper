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
}
