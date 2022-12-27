using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Library.Exporters.Scripts;

public partial class ScriptExportCollection
{
	private readonly record struct MonoScriptInfo(string Class, string Namespace, string Assembly)
	{
		public static MonoScriptInfo From(IMonoScript monoScript)
		{
			return new MonoScriptInfo(monoScript.ClassName_C115.String, monoScript.Namespace_C115.String, monoScript.GetAssemblyNameFixed());
		}
	}
}
