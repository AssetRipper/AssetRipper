using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using System.Numerics;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass202_VectorExplicitConversions
{
	public static void DoPass()
	{
		//DoImplementation<Vector3f, IVector3f>(SharedState.Instance.SubclassGroups["Vector3Float"], 3);
		//DoImplementation<Vector4f, IVector4f>(SharedState.Instance.SubclassGroups["Vector4Float"], 4);
		//DoImplementation<Vector2i, IVector2i>(SharedState.Instance.SubclassGroups["Vector2Int"], 2);
		//DoImplementation<Vector3i, IVector3i>(SharedState.Instance.SubclassGroups["Vector3Int"], 3);
		//DoImplementation<Vector2f, IVector2f>(SharedState.Instance.SubclassGroups["Vector2f"], 2);
		//DoImplementation<Vector3f, IVector3f>(SharedState.Instance.SubclassGroups["Vector3f"], 3);
		//DoImplementation<Vector4f, IVector4f>(SharedState.Instance.SubclassGroups["Vector4f"], 4);
		//DoImplementation<Quaternionf, IQuaternionf>(SharedState.Instance.SubclassGroups["Quaternionf"], 4);

		DoImplementation<Vector3>(SharedState.Instance.SubclassGroups["Vector3Float"], 3);
		DoImplementation<Vector4>(SharedState.Instance.SubclassGroups["Vector4Float"], 4);
		DoImplementation<Vector2>(SharedState.Instance.SubclassGroups["Vector2f"], 2);
		DoImplementation<Vector3>(SharedState.Instance.SubclassGroups["Vector3f"], 3);
		DoImplementation<Vector4>(SharedState.Instance.SubclassGroups["Vector4f"], 4);
		DoImplementation<Quaternion>(SharedState.Instance.SubclassGroups["Quaternionf"], 4);
	}

	//private static void DoImplementation<TClass, TInterface>(SubclassGroup group, int size)
	//{
	//	AddInterface<TInterface>(group, size);
	//	foreach (TypeDefinition type in group.Types)
	//	{
	//		AddConversion<TClass>(type, size);
	//		AddReverseConversion<TClass>(type, size);
	//	}
	//}

	private static void DoImplementation<TClass>(SubclassGroup group, int size)
	{
		foreach (TypeDefinition type in group.Types)
		{
			AddConversion<TClass>(type, size);
			AddReverseConversion<TClass>(type, size);
		}
	}

	private static void AddConversion<T>(TypeDefinition type, int size)
	{
		TypeSignature commonType = SharedState.Instance.Importer.ImportTypeSignature<T>();
		IMethodDefOrRef constructor = SharedState.Instance.Importer.ImportConstructor<T>(size);

		MethodDefinition method = type.AddEmptyConversion(type.ToTypeSignature(), commonType, true);
		CilInstructionCollection instructions = method.GetInstructions();

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, type.Fields.Single(field => field.Name == "m_X"));

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, type.Fields.Single(field => field.Name == "m_Y"));

		if (size > 2)
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, type.Fields.Single(field => field.Name == "m_Z"));
		}
		if (size > 3)
		{
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, type.Fields.Single(field => field.Name == "m_W"));
		}

		instructions.Add(CilOpCodes.Newobj, constructor);
		instructions.Add(CilOpCodes.Ret);
	}

	private static void AddReverseConversion<T>(TypeDefinition type, int size)
	{
		TypeSignature commonType = SharedState.Instance.Importer.ImportTypeSignature<T>();

		MethodDefinition constructor = type.GetDefaultConstructor();

		MethodDefinition method = type.AddEmptyConversion(commonType, type.ToTypeSignature(), false);
		CilInstructionCollection instructions = method.GetInstructions();

		instructions.Add(CilOpCodes.Newobj, constructor);

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, SharedState.Instance.Importer.ImportField<T>("X"));
		instructions.Add(CilOpCodes.Stfld, type.Fields.Single(field => field.Name == "m_X"));

		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, SharedState.Instance.Importer.ImportField<T>("Y"));
		instructions.Add(CilOpCodes.Stfld, type.Fields.Single(field => field.Name == "m_Y"));

		if (size > 2)
		{
			instructions.Add(CilOpCodes.Dup);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, SharedState.Instance.Importer.ImportField<T>("Z"));
			instructions.Add(CilOpCodes.Stfld, type.Fields.Single(field => field.Name == "m_Z"));
		}
		if (size > 3)
		{
			instructions.Add(CilOpCodes.Dup);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, SharedState.Instance.Importer.ImportField<T>("W"));
			instructions.Add(CilOpCodes.Stfld, type.Fields.Single(field => field.Name == "m_W"));
		}

		instructions.Add(CilOpCodes.Ret);
	}

	private static void AddInterface<T>(SubclassGroup group, int size)
	{
		group.Interface.AddInterfaceImplementation<T>(SharedState.Instance.Importer);
		foreach (TypeDefinition type in group.Types)
		{
			type.ImplementVectorProperty("X");
			type.ImplementVectorProperty("Y");
			if (size > 2)
			{
				type.ImplementVectorProperty("Z");
			}
			if (size > 3)
			{
				type.ImplementVectorProperty("W");
			}
		}
	}

	private static void ImplementVectorProperty(this TypeDefinition type, string propertyName)
	{
		type.ImplementFullProperty(
			propertyName,
			InterfaceUtils.InterfacePropertyImplementation,
			null,
			type.GetFieldByName($"m_{propertyName}"));
	}
}
