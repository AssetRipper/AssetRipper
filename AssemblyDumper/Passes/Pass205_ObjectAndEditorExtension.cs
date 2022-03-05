using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.PrefabInstance;

namespace AssemblyDumper.Passes
{
	public static class Pass205_ObjectAndEditorExtension
	{
		const MethodAttributes PropertyOverrideAttributes =
			MethodAttributes.Public |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.ReuseSlot |
			MethodAttributes.Virtual |
			MethodAttributes.Final;
		const string FieldName = "m_ObjectHideFlags";

		public static void DoPass()
		{
			Console.WriteLine("Pass 205: Object and Editor Extension");
			SharedState.TypeDictionary["Object"].AddHideFlagsProperty();
			SharedState.TypeDictionary["EditorExtension"].ImplementEditorExtensionInterface();
		}

		private static void AddHideFlagsProperty(this TypeDefinition type)
		{
			ITypeDefOrRef hideFlagsEnum = SharedState.Importer.ImportCommonType<AssetRipper.Core.Classes.Object.HideFlags>();
			type.ImplementFullProperty("ObjectHideFlags", PropertyOverrideAttributes, hideFlagsEnum.ToTypeSignature(), type.GetFieldByName(FieldName));
		}

		private static void ImplementEditorExtensionInterface(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IEditorExtension>();
			TypeSignature returnType = InterfaceUtils.GetPropertyTypeSignature<IEditorExtension>(nameof(IEditorExtension.PrefabInstance));
			if(type.TryGetFieldByName("m_PrefabInternal", out FieldDefinition? field) || type.TryGetFieldByName("m_PrefabInstance", out field))
			{
				TypeSignature fieldType = field.Signature!.FieldType;
				PropertyDefinition property = type.AddFullProperty(nameof(IEditorExtension.PrefabInstance), InterfaceUtils.InterfacePropertyImplementation, returnType);
				MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<IPrefabInstance>(fieldType.Resolve()!);
				
				CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
				getProcessor.Add(CilOpCodes.Ldarg_0);
				getProcessor.Add(CilOpCodes.Ldfld, field);
				getProcessor.Add(CilOpCodes.Call, explicitConversionMethod);
				getProcessor.Add(CilOpCodes.Ret);

				CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
				IMethodDefOrRef pptrSetMethod = SharedState.Importer.ImportCommonMethod(typeof(PPtr), m => m.Name == "SetValues");
				setProcessor.Add(CilOpCodes.Ldarg_0);
				setProcessor.Add(CilOpCodes.Ldfld, field);
				setProcessor.Add(CilOpCodes.Ldarg_1);
				setProcessor.Add(CilOpCodes.Call, pptrSetMethod);
				setProcessor.Add(CilOpCodes.Ret);
			}
			else
			{
				type.ImplementFullProperty(nameof(IEditorExtension.PrefabInstance), InterfaceUtils.InterfacePropertyImplementation, returnType, null);
			}
		}
	}
}
