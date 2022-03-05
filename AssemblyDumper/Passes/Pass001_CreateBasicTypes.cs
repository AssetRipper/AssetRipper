using AsmResolver.DotNet.Signatures;
using AssemblyDumper.Unity;
using AssemblyDumper.Utils;

namespace AssemblyDumper.Passes
{
	public static class Pass001_CreateBasicTypes
	{
		private const TypeAttributes StaticClassAttributes = TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract;
		private const MethodAttributes StaticConstructorAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.RuntimeSpecialName | MethodAttributes.SpecialName | MethodAttributes.Static;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static TypeDefinition ClassIDTypeDefinition;
		public static TypeDefinition CommonStringTypeDefinition;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static void DoPass()
		{
			System.Console.WriteLine("Pass 001: Create Basic Types");
			ClassIDTypeDefinition = CreateClassID();
			CommonStringTypeDefinition = CreateCommonStringClass();
		}

		private static TypeDefinition CreateClassID()
		{
			Dictionary<string, int> classIdDictionary = SharedState.ClassDictionary.Values.ToDictionary(x => x.Name!, x => x.TypeID);
			return EnumCreator.CreateFromDictionary(SharedState.Assembly, SharedState.RootNamespace, "ClassIDType", classIdDictionary);
		}

		private static TypeDefinition CreateCommonStringClass()
		{
			TypeDefinition newTypeDef = CreateEmptyStaticClass(SharedState.Assembly, SharedState.RootNamespace, "CommonString");

			GenericInstanceTypeSignature? uintStringDictionary = SystemTypeGetter.Dictionary.MakeGenericInstanceType(SystemTypeGetter.UInt32, SystemTypeGetter.String);
			IMethodDefOrRef? dictionaryConstructor = MethodUtils.MakeConstructorOnGenericType(uintStringDictionary, 0);
			IMethodDefOrRef? addMethod = MethodUtils.MakeMethodOnGenericType(uintStringDictionary, uintStringDictionary.Resolve()!.Methods.First(m => m.Name == "Add"));

			FieldDefinition field = new FieldDefinition("dictionary", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, FieldSignature.CreateStatic(uintStringDictionary));
			newTypeDef.Fields.Add(field);

			MethodDefinition? staticConstructor = new MethodDefinition(".cctor", StaticConstructorAttributes, MethodSignature.CreateStatic(SystemTypeGetter.Void));
			newTypeDef.Methods.Add(staticConstructor);
			staticConstructor.CilMethodBody = new CilMethodBody(staticConstructor);
			CilInstructionCollection processor = staticConstructor.CilMethodBody.Instructions;
			processor.Add(CilOpCodes.Newobj, dictionaryConstructor);
			foreach(UnityString unityString in SharedState.Strings)
			{
				processor.Add(CilOpCodes.Dup);
				processor.Add(CilOpCodes.Ldc_I4, (int)unityString.Index);
				processor.Add(CilOpCodes.Ldstr, unityString.String!);
				processor.Add(CilOpCodes.Call, addMethod);
			}
			processor.Add(CilOpCodes.Stsfld, field);
			processor.Add(CilOpCodes.Ret);

			processor.OptimizeMacros();

			return newTypeDef;
		}

		private static TypeDefinition CreateEmptyStaticClass(AssemblyDefinition assembly, string @namespace, string name)
		{
			TypeDefinition newTypeDef = new TypeDefinition(@namespace, name, StaticClassAttributes);
			newTypeDef.BaseType = SystemTypeGetter.Object.ToTypeDefOrRef();
			assembly.ManifestModule!.TopLevelTypes.Add(newTypeDef);

			return newTypeDef;
		}
	}
}
