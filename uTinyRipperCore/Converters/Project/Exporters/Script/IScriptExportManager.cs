using Mono.Cecil;
using System.Collections.Generic;
using uTinyRipper.Layout;

namespace uTinyRipper.Converters.Script
{
	public interface IScriptExportManager
	{
		ScriptExportType RetrieveType(TypeReference type);
		ScriptExportEnum RetrieveEnum(TypeDefinition type);
		ScriptExportDelegate RetrieveDelegate(TypeDefinition type);
		ScriptExportAttribute RetrieveAttribute(CustomAttribute attribute);
		ScriptExportMethod RetrieveMethod(MethodDefinition method);
		ScriptExportConstructor RetrieveConstructor(MethodDefinition constructor);
		ScriptExportProperty RetrieveProperty(PropertyDefinition property);
		ScriptExportField RetrieveField(FieldDefinition field);
		ScriptExportParameter RetrieveParameter(ParameterDefinition parameter);

		AssetLayout Layout { get; }

		IEnumerable<ScriptExportType> Types { get; }
		IEnumerable<ScriptExportEnum> Enums { get; }
		IEnumerable<ScriptExportDelegate> Delegates { get; }
	}
}
