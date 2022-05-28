using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Library.Exporters.Scripts
{
	public partial class ScriptExportCollection
	{
		private struct MonoScriptInfo : IEquatable<MonoScriptInfo>
		{
			public readonly string @class;
			public readonly string @namespace;
			public readonly string assembly;

			public MonoScriptInfo(string @class, string @namespace, string assembly)
			{
				this.@class = @class;
				this.@namespace = @namespace;
				this.assembly = assembly;
			}

			public static MonoScriptInfo From(IMonoScript monoScript)
			{
				return new MonoScriptInfo(monoScript.ClassName_C115.String, monoScript.Namespace_C115.String, monoScript.GetAssemblyNameFixed());
			}

			public override bool Equals(object? obj)
			{
				return obj is MonoScriptInfo info && Equals(info);
			}

			public bool Equals(MonoScriptInfo other)
			{
				return @class == other.@class &&
					   @namespace == other.@namespace &&
					   assembly == other.assembly;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(@class, @namespace, assembly);
			}

			public static bool operator ==(MonoScriptInfo left, MonoScriptInfo right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(MonoScriptInfo left, MonoScriptInfo right)
			{
				return !(left == right);
			}
		}
	}
}
