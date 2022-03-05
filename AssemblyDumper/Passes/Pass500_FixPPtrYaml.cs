using AssemblyDumper.Utils;

namespace AssemblyDumper.Passes
{
	public static class Pass500_FixPPtrYaml
	{
		public static void DoPass()
		{
			System.Console.WriteLine("Pass 500: Fix PPtr Yaml Export");

			foreach (string name in SharedState.ClassDictionary.Keys)
			{
				if (name.StartsWith("PPtr_"))
				{
					string parameterTypeName = name.Substring(5, name.LastIndexOf('_') - 5);
					TypeDefinition parameterType = SharedState.TypeDictionary[parameterTypeName];
					TypeDefinition pptrType = SharedState.TypeDictionary[name];
					FixYaml(pptrType, parameterType);
				}
			}
		}

		private static void FixYaml(TypeDefinition pptrType, TypeDefinition parameterType)
		{
			Func<MethodDefinition, bool> filter = m => m.Name == "ExportYAML" && m.Parameters.Count == 3;
			IMethodDefOrRef exportGeneric = SharedState.Importer.ImportCommonMethod("AssetRipper.Core.Classes.Misc.PPtr", filter);
			MethodSpecification commonExportReference = MethodUtils.MakeGenericInstanceMethod(exportGeneric, parameterType.ToTypeSignature());
			
			MethodDefinition releaseYamlMethod = pptrType.Methods.Single(m => m.Name == "ExportYAMLRelease");
			MethodDefinition editorYamlMethod = pptrType.Methods.Single(m => m.Name == "ExportYAMLEditor");

			FixMethod(releaseYamlMethod, commonExportReference);
			FixMethod(editorYamlMethod, commonExportReference);
		}

		private static void FixMethod(MethodDefinition yamlMethod, MethodSpecification exportMethod)
		{
			FieldDefinition fileID = yamlMethod.DeclaringType!.Fields.Single(f => f.Name == "m_FileID");
			FieldDefinition pathID = yamlMethod.DeclaringType.Fields.Single(f => f.Name == "m_PathID");
			CilInstructionCollection processor = yamlMethod.CilMethodBody!.Instructions;
			processor.Clear();
			processor.Add(CilOpCodes.Ldarg_1);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, fileID);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, pathID);
			processor.Add(CilOpCodes.Call, exportMethod);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
