using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Font;

namespace AssemblyDumper.Passes
{
	public static class Pass362_MiscellaneousExporters
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 362: Miscellaneous Exporters");

			SharedState.TypeDictionary["TextAsset"].ImplementTextAsset();
			SharedState.TypeDictionary["Font"].ImplementFontAsset();
			SharedState.TypeDictionary["MovieTexture"].ImplementMovieTexture();
		}

		private static void ImplementFontAsset(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IFont>();
			type.ImplementFullProperty(nameof(IFont.FontData), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.UInt8.MakeSzArrayType(), type.GetFieldByName("m_FontData"));
		}

		private static void ImplementMovieTexture(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IMovieTexture>();
			type.ImplementFullProperty(nameof(IMovieTexture.MovieData), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.UInt8.MakeSzArrayType(), type.TryGetFieldByName("m_MovieData", true));
		}

		private static void ImplementTextAsset(this TypeDefinition type)
		{
			FieldDefinition scriptField = type.GetFieldByName("m_Script");
			TypeDefinition textAssetContentType = SharedState.TypeDictionary["TextAssetContent"];
			FieldDefinition contentField = textAssetContentType.GetFieldByName("content");
			
			type.ImplementTextAssetInterface(scriptField, contentField);
			textAssetContentType.FixTextAssetContentYaml(contentField);
		}

		private static void ImplementTextAssetInterface(this TypeDefinition type, FieldDefinition scriptField, FieldDefinition contentField)
		{
			type.AddInterfaceImplementation<ITextAsset>();
			PropertyDefinition property = type.AddGetterProperty(nameof(ITextAsset.Script), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.UInt8.MakeSzArrayType());
			CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, scriptField);
			processor.Add(CilOpCodes.Ldfld, contentField);
			processor.Add(CilOpCodes.Ret);
		}

		private static void FixTextAssetContentYaml(this TypeDefinition type, FieldDefinition contentField)
		{
			Func<MethodDefinition, bool> filter1 = m => m.Name == "ExportYAML" && m.Parameters.Count == 1;
			IMethodDefOrRef byteArrayToYamlMethod = SharedState.Importer.ImportCommonMethod(typeof(AssetRipper.Core.YAML.Extensions.ArrayYAMLExtensions), filter1);
			MethodDefinition editorMethod = type.Methods.Single(m => m.Name == "ExportYAMLEditor");
			MethodDefinition releaseMethod = type.Methods.Single(m => m.Name == "ExportYAMLRelease");
			FixTextAssetContentYaml(editorMethod, contentField, byteArrayToYamlMethod);
			FixTextAssetContentYaml(releaseMethod, contentField, byteArrayToYamlMethod);
		}

		private static void FixTextAssetContentYaml(MethodDefinition yamlMethod, FieldDefinition contentField, IMethodDefOrRef exportMethod)
		{
			CilMethodBody methodBody = yamlMethod.CilMethodBody!;
			methodBody.InitializeLocals = false;
			methodBody.LocalVariables.Clear();
			CilInstructionCollection processor = methodBody.Instructions;
			processor.Clear();
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, contentField);
			processor.Add(CilOpCodes.Call, exportMethod);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
