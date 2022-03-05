using AssemblyDumper.Utils;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Classes.Shader.SerializedShader;
using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;

namespace AssemblyDumper.Passes
{
	public static class Pass363_ShaderInterfaces
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 363: Shader Interfaces");
			if(SharedState.TypeDictionary.TryGetValue("SerializedTextureProperty", out TypeDefinition? serializedTextureProperty))
			{
				serializedTextureProperty.ImplementSerializedTextureProperty();
			}
			if (SharedState.TypeDictionary.TryGetValue("SerializedProperty", out TypeDefinition? serializedProperty))
			{
				serializedProperty.ImplementSerializedProperty();
			}
			if (SharedState.TypeDictionary.TryGetValue("SerializedProperties", out TypeDefinition? serializedProperties))
			{
				serializedProperties.ImplementSerializedProperties();
			}
			if (SharedState.TypeDictionary.TryGetValue("SerializedShader", out TypeDefinition? serializedShader))
			{
				serializedShader.ImplementSerializedShader();
			}
			if (SharedState.TypeDictionary.TryGetValue("Shader", out TypeDefinition? shader))
			{
				shader.ImplementShader();
			}
		}

		private static void ImplementSerializedTextureProperty(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<ISerializedTextureProperty>();
			type.ImplementStringProperty(nameof(ISerializedTextureProperty.DefaultName), InterfaceUtils.InterfacePropertyImplementation, type.TryGetFieldByName("m_DefaultName"));
			type.ImplementFullProperty(nameof(ISerializedTextureProperty.TexDim), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, type.TryGetFieldByName("m_TexDim"));
		}

		private static void ImplementSerializedProperty(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<ISerializedProperty>();
			type.ImplementStringProperty(nameof(ISerializedProperty.Description), InterfaceUtils.InterfacePropertyImplementation, type.TryGetFieldByName("m_Description"));
			
			type.ImplementGetterProperty(
				nameof(ISerializedProperty.Attributes), 
				InterfaceUtils.InterfacePropertyImplementation, 
				SharedState.Importer.ImportCommonType<AssetRipper.Core.Classes.Utf8StringBase>().MakeSzArrayType(), 
				type.TryGetFieldByName("m_Attributes"));

			type.ImplementFullProperty(
				nameof(ISerializedProperty.Type), 
				InterfaceUtils.InterfacePropertyImplementation, 
				SharedState.Importer.ImportCommonType<SerializedPropertyType>().ToTypeSignature(), 
				type.TryGetFieldByName("m_Type"));

			type.ImplementFullProperty(
				nameof(ISerializedProperty.Flags),
				InterfaceUtils.InterfacePropertyImplementation,
				SharedState.Importer.ImportCommonType<SerializedPropertyFlag>().ToTypeSignature(),
				type.TryGetFieldByName("m_Flags"));

			type.ImplementFullProperty(nameof(ISerializedProperty.DefValue0), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Single, type.TryGetFieldByName("m_DefValue_0_"));
			type.ImplementFullProperty(nameof(ISerializedProperty.DefValue1), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Single, type.TryGetFieldByName("m_DefValue_1_"));
			type.ImplementFullProperty(nameof(ISerializedProperty.DefValue2), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Single, type.TryGetFieldByName("m_DefValue_2_"));
			type.ImplementFullProperty(nameof(ISerializedProperty.DefValue3), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Single, type.TryGetFieldByName("m_DefValue_3_"));
			type.ImplementGetterProperty(
				nameof(ISerializedProperty.DefTexture), 
				InterfaceUtils.InterfacePropertyImplementation, 
				SharedState.Importer.ImportCommonType<ISerializedTextureProperty>().ToTypeSignature(), 
				type.TryGetFieldByName("m_DefTexture"));
		}

		private static void ImplementSerializedProperties(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<ISerializedProperties>();
			type.ImplementGetterProperty(
				nameof(ISerializedProperties.Props),
				InterfaceUtils.InterfacePropertyImplementation,
				SharedState.Importer.ImportCommonType<ISerializedProperty>().MakeSzArrayType(),
				type.GetFieldByName("m_Props"));
		}

		private static void ImplementSerializedShader(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<ISerializedShader>();
			type.ImplementStringProperty(nameof(ISerializedShader.CustomEditorName), InterfaceUtils.InterfacePropertyImplementation, type.TryGetFieldByName("m_CustomEditorName"));
			type.ImplementStringProperty(nameof(ISerializedShader.FallbackName), InterfaceUtils.InterfacePropertyImplementation, type.TryGetFieldByName("m_FallbackName"));
			type.ImplementGetterProperty(
				nameof(ISerializedShader.PropInfo),
				InterfaceUtils.InterfacePropertyImplementation,
				SharedState.Importer.ImportCommonType<ISerializedProperties>().ToTypeSignature(),
				type.GetFieldByName("m_PropInfo"));
		}

		private static void ImplementShader(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IShader>();
			type.ImplementHasFieldProperty(nameof(IShader.HasParsedForm), InterfaceUtils.InterfacePropertyImplementation, "m_ParsedForm");
			type.ImplementGetterProperty(
				nameof(IShader.ParsedForm),
				InterfaceUtils.InterfacePropertyImplementation,
				SharedState.Importer.ImportCommonType<ISerializedShader>().ToTypeSignature(),
				type.TryGetFieldByName("m_ParsedForm"));
		}
	}
}
