using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.ResourceManager;
using AssetRipper.Core.Classes.TagManager;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;

namespace AssemblyDumper.Passes
{
	public static class Pass341_ManagerInterfaces
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 341: Manager Interfaces");
			ImplementTagManager();
			ImplementResourceManager();
		}

		private static void ImplementTagManager()
		{
			TypeDefinition type = SharedState.TypeDictionary["TagManager"];
			type.AddInterfaceImplementation<ITagManager>();
			type.ImplementGetterProperty(
				nameof(ITagManager.Tags), 
				InterfaceUtils.InterfacePropertyImplementation, 
				SharedState.Importer.ImportCommonType<Utf8StringBase>().MakeSzArrayType(), 
				type.GetFieldByName("tags"));
		}

		private static void ImplementResourceManager()
		{
			TypeDefinition type = SharedState.TypeDictionary["ResourceManager"];
			type.AddInterfaceImplementation<IResourceManager>();
			TypeSignature? returnTypeSignature = SharedState.Importer.ImportTypeSignature(CommonTypeGetter.LookupCommonType<IResourceManager>()!.Methods.Single().Signature!.ReturnType);
			MethodDefinition method = type.AddMethod(nameof(IResourceManager.GetAssets), InterfaceUtils.InterfaceMethodImplementation, returnTypeSignature);

			FieldDefinition field = type.GetFieldByName("m_Container");
			GenericInstanceTypeSignature fieldType = (GenericInstanceTypeSignature)field.Signature!.FieldType;
			TypeSignature typeArgument1 = fieldType.TypeArguments[0];
			TypeSignature typeArgument2 = fieldType.TypeArguments[1];
			TypeSignature typeArgument3 = SharedState.Importer.ImportCommonType<IUnityObjectBase>().ToTypeSignature();
			IMethodDefOrRef extensionMethod = SharedState.Importer.ImportCommonMethod(typeof(AssetDictionaryExtensions), m => m.Name == nameof(AssetDictionaryExtensions.ToPPtrArray));
			MethodSpecification extensionMethodInstance = MethodUtils.MakeGenericInstanceMethod(extensionMethod, typeArgument1, typeArgument2, typeArgument3);

			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, field);
			processor.Add(CilOpCodes.Call, extensionMethodInstance);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
