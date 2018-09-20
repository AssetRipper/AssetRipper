using Mono.Cecil;
using System.Collections.Generic;

namespace uTinyRipper.Exporters.Scripts
{
	public interface IScriptExportManager
	{
		ScriptExportType RetrieveType(TypeReference type);
		ScriptExportEnum RetrieveEnum(TypeDefinition type);
		ScriptExportDelegate RetrieveDelegate(TypeDefinition type);
		ScriptExportAttribute RetrieveAttribute(CustomAttribute attribute);
		ScriptExportField RetrieveField(FieldDefinition field);
		ScriptExportParameter RetrieveParameter(ParameterDefinition parameter);

		IEnumerable<ScriptExportType> Types { get; }
		IEnumerable<ScriptExportEnum> Enums { get; }
		IEnumerable<ScriptExportDelegate> Delegates { get; }
	}
}
