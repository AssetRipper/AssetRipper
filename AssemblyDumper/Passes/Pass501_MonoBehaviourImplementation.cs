using AsmResolver.DotNet.Signatures;
using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;

namespace AssemblyDumper.Passes
{
	/// <summary>
	/// Implements the IMonoBehaviour interface. Also fixes the read and yaml methods
	/// </summary>
	public static class Pass501_MonoBehaviourImplementation
	{
		const MethodAttributes InterfacePropertyImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static TypeSignature serializableStructureSignature;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static void DoPass()
		{
			Console.WriteLine("Pass 501: MonoBehaviour Implementation");
			ITypeDefOrRef monobehaviourInterface = SharedState.Importer.ImportCommonType<IMonoBehaviour>();
			serializableStructureSignature = SharedState.Importer.ImportCommonType<AssetRipper.Core.Structure.Assembly.Serializable.SerializableStructure>().ToTypeSignature();
			if (SharedState.TypeDictionary.TryGetValue("MonoBehaviour", out TypeDefinition? type))
			{
				type.Interfaces.Add(new InterfaceImplementation(monobehaviourInterface));
				FieldDefinition structureField = type.AddStructureField();
				type.ImplementFullProperty("Structure", InterfacePropertyImplementationAttributes, null, structureField);
				type.AddScriptPtrProperty();
				type.FixReadMethods();
				type.FixExportMethods();
				type.FixFetchDependencies();
			}
			else
			{
				throw new Exception("MonoBehaviour not found");
			}
		}

		private static FieldDefinition AddStructureField(this TypeDefinition type)
		{
			FieldDefinition result = new FieldDefinition("_structure", FieldAttributes.Private, FieldSignature.CreateInstance(serializableStructureSignature));
			type.Fields.Add(result);
			return result;
		}

		private static PropertyDefinition AddScriptPtrProperty(this TypeDefinition type)
		{
			FieldDefinition field = type.GetFieldByName("m_Script");
			TypeSignature fieldType = field.Signature!.FieldType;
			MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<IMonoScript>(fieldType.Resolve()!);
			PropertyDefinition property = type.AddGetterProperty("ScriptPtr", InterfacePropertyImplementationAttributes, explicitConversionMethod.Signature!.ReturnType);
			CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, field);
			processor.Add(CilOpCodes.Call, explicitConversionMethod);
			processor.Add(CilOpCodes.Ret);
			return property;
		}

		private static void FixReadMethods(this TypeDefinition type)
		{
			MethodDefinition readRelease = type.Methods.Single(m => m.Name == "ReadRelease");
			readRelease.AddReadStructure();
			MethodDefinition readEditor = type.Methods.Single(m => m.Name == "ReadEditor");
			readEditor.AddReadStructure();
		}

		private static void AddReadStructure(this MethodDefinition method)
		{
			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Pop();//Remove the return
			IMethodDefOrRef readStructureMethod = SharedState.Importer.ImportCommonMethod(typeof(MonoBehaviourExtensions), m => m.Name == "ReadStructure");
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldarg_1);
			processor.Add(CilOpCodes.Call, readStructureMethod);
			processor.Add(CilOpCodes.Ret);
		}

		private static void FixExportMethods(this TypeDefinition type)
		{
			MethodDefinition exportRelease = type.Methods.Single(m => m.Name == "ExportYAMLRelease");
			exportRelease.AddExportStructureYaml();
			MethodDefinition exportEditor = type.Methods.Single(m => m.Name == "ExportYAMLEditor");
			exportEditor.AddExportStructureYaml();
		}

		private static void AddExportStructureYaml(this MethodDefinition method)
		{
			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Pop();//Remove the return
			processor.Pop();//Remove the mapping node load
			IMethodDefOrRef exportStructureMethod = SharedState.Importer.ImportCommonMethod(typeof(MonoBehaviourExtensions), m => m.Name == "MaybeExportYamlForStructure");
			processor.Add(CilOpCodes.Ldarg_0);//this
			processor.Add(CilOpCodes.Ldloc_0);//mapping node
			processor.Add(CilOpCodes.Ldarg_1);//container
			processor.Add(CilOpCodes.Call, exportStructureMethod);
			processor.Add(CilOpCodes.Ldloc_0);//mapping node
			processor.Add(CilOpCodes.Ret);
		}

		private static void FixFetchDependencies(this TypeDefinition type)
		{
			MethodDefinition method = type.Methods.Single(m => m.Name == "FetchDependencies");
			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Pop();//Remove the return
			IMethodDefOrRef fetchStructureMethod = SharedState.Importer.ImportCommonMethod(typeof(MonoBehaviourExtensions), 
				m => m.Name == nameof(MonoBehaviourExtensions.MaybeFetchDependenciesForStructure));

			processor.Add(CilOpCodes.Ldarg_0);//this
			processor.Add(CilOpCodes.Ldarg_1);//context
			processor.Add(CilOpCodes.Call, fetchStructureMethod);
			processor.Add(CilOpCodes.Call, Pass103_FillDependencyMethods.unityObjectBasePPtrListAddRange);
			processor.Add(CilOpCodes.Ldloc_0);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
