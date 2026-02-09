using AssetRipper.AssemblyDumper.AST;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Traversal;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass108_WalkMethods
{
	private enum State
	{
		Release,
		Editor,
		Standard,
	}

	private static State CurrentState { get; set; } = State.Standard;

	private static string MethodName => CurrentState switch
	{
		State.Release => nameof(IUnityAssetBase.WalkRelease),
		State.Editor => nameof(IUnityAssetBase.WalkEditor),
		_ => nameof(IUnityAssetBase.WalkStandard),
	};

	private static bool IsUsable(FieldNode node) => CurrentState switch
	{
		State.Release => !node.Property.IsEditorOnly && !node.Property.IsInjected,
		State.Editor => !node.Property.IsReleaseOnly && !node.Property.IsInjected,
		_ => true,
	};

	private static List<FieldNode> ToOrderedList(this IEnumerable<FieldNode> nodes)
	{
		switch (CurrentState)
		{
			case State.Release:
				{
					List<FieldNode> list = nodes.ToList();
					if (list.Count > 0)
					{
						UniversalNode root = list[0].Property.Class.Class.ReleaseRootNode!;
						list.Sort((a, b) => root.SubNodes.IndexOf(a.Property.ReleaseNode!) - root.SubNodes.IndexOf(b.Property.ReleaseNode!));
					}
					return list;
				}
			case State.Editor:
				{
					List<FieldNode> list = nodes.ToList();
					if (list.Count > 0)
					{
						UniversalNode root = list[0].Property.Class.Class.EditorRootNode!;
						list.Sort((a, b) => root.SubNodes.IndexOf(a.Property.EditorNode!) - root.SubNodes.IndexOf(b.Property.EditorNode!));
					}
					return list;
				}
			default:
				return nodes.OrderBy(n => n.Field.Name?.Value).ToList();
		}
	}

	private static string GetName(FieldNode node) => CurrentState switch
	{
		State.Release or State.Editor => node.Property.OriginalFieldName!,
		_ => node.Field.Name!,
	};
#nullable disable
	private static TypeSignature assetWalkerType;

	private static IMethodDefOrRef enterAssetMethod;
	private static IMethodDefOrRef divideAssetMethod;
	private static IMethodDefOrRef exitAssetMethod;

	private static IMethodDefOrRef enterFieldMethod;
	private static IMethodDefOrRef exitFieldMethod;

	private static IMethodDefOrRef enterListMethod;
	private static IMethodDefOrRef divideListMethod;
	private static IMethodDefOrRef exitListMethod;

	private static IMethodDefOrRef enterDictionaryMethod;
	private static IMethodDefOrRef divideDictionaryMethod;
	private static IMethodDefOrRef exitDictionaryMethod;

	private static IMethodDefOrRef enterDictionaryPairMethod;
	private static IMethodDefOrRef divideDictionaryPairMethod;
	private static IMethodDefOrRef exitDictionaryPairMethod;

	private static IMethodDefOrRef enterPairMethod;
	private static IMethodDefOrRef dividePairMethod;
	private static IMethodDefOrRef exitPairMethod;

	private static IMethodDefOrRef visitPrimitiveMethod;
	private static IMethodDefOrRef visitPPtrMethod;

	private static TypeDefinition helperClass;
#nullable enable
	private static Dictionary<TypeSignature, IMethodDescriptor> MethodDictionary { get; } = new(SignatureComparer.Default);

	public static void DoPass()
	{
		Initialize();
		foreach (State state in (ReadOnlySpan<State>)[State.Release, State.Editor, State.Standard])
		{
			CurrentState = state;
			MethodDictionary.Clear();

			CreateEmptyMethods();

			helperClass = StaticClassCreator.CreateEmptyStaticClass(SharedState.Instance.Module, SharedState.HelpersNamespace, MethodName + "Methods");
			helperClass.IsPublic = false;

			foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
			{
				foreach (GeneratedClassInstance instance in group.Instances)
				{
					TypeNode rootNode = new(instance);

					TypeDefinition type = instance.Type;
					TypeSignature typeSignature = type.ToTypeSignature();

					CilInstructionCollection instructions = type.GetMethodByName(MethodName).GetInstructions();

					if (group.IsPPtr)
					{
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Callvirt, visitPPtrMethod.MakeGenericInstanceMethod(Pass080_PPtrConversions.PPtrsToParameters[type].ToTypeSignature()));
						instructions.Add(CilOpCodes.Ret);
					}
					else
					{
						CilInstructionLabel returnLabel = new();

						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Callvirt, enterAssetMethod);
						instructions.Add(CilOpCodes.Brfalse, returnLabel);

						List<FieldNode> usableChildren = rootNode.Children.Where(IsUsable).ToOrderedList();
						for (int i = 0; i < usableChildren.Count; i++)
						{
							FieldNode fieldNode = usableChildren[i];
							CilInstructionLabel finishLabel = new();

							if (i > 0)
							{
								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Callvirt, divideAssetMethod);
							}

							string fieldName;
							if ((CurrentState is State.Release && fieldNode.Property.ReleaseNode?.NodeType is NodeType.TypelessData)
								|| (CurrentState is State.Editor && fieldNode.Property.ReleaseNode?.NodeType is NodeType.TypelessData))
							{
								//This is required for correct yaml export

								Debug.Assert(fieldNode.TypeSignature is SzArrayTypeSignature);

								string lengthName = GetName(fieldNode);

								CilInstructionLabel lengthLabel = new();

								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Ldstr, lengthName);
								instructions.Add(CilOpCodes.Callvirt, enterFieldMethod);
								instructions.Add(CilOpCodes.Brfalse, lengthLabel);

								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Ldfld, fieldNode.Field);
								instructions.Add(CilOpCodes.Ldlen);
								instructions.Add(CilOpCodes.Callvirt, visitPrimitiveMethod.MakeGenericInstanceMethod(SharedState.Instance.Importer.Int32));

								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Ldstr, lengthName);
								instructions.Add(CilOpCodes.Callvirt, exitFieldMethod);

								lengthLabel.Instruction = instructions.Add(CilOpCodes.Nop);

								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Callvirt, divideAssetMethod);

								fieldName = "_typelessdata";
							}
							else
							{
								fieldName = GetName(fieldNode);
							}

							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldstr, fieldName);
							instructions.Add(CilOpCodes.Callvirt, enterFieldMethod);
							instructions.Add(CilOpCodes.Brfalse, finishLabel);

							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldfld, fieldNode.Field);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.AddCall(GetOrMakeMethod(fieldNode.Child));

							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldstr, fieldName);
							instructions.Add(CilOpCodes.Callvirt, exitFieldMethod);

							finishLabel.Instruction = instructions.Add(CilOpCodes.Nop);
						}

						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Callvirt, exitAssetMethod);

						returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
					}
				}
			}
		}
	}

	private static IMethodDescriptor GetOrMakeMethod(Node node)
	{
		if (MethodDictionary.TryGetValue(node.TypeSignature, out IMethodDescriptor? cachedMethod))
		{
			return cachedMethod;
		}

		IMethodDescriptor result;
		switch (node)
		{
			case PrimitiveNode:
				{
					MethodDefinition method = NewMethod(node);
					CilInstructionCollection instructions = method.GetInstructions();
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Callvirt, visitPrimitiveMethod.MakeGenericInstanceMethod(node.TypeSignature));
					instructions.Add(CilOpCodes.Ret);
					result = method;
				}
				break;
			case ListNode listNode:
				{
					MethodDefinition method = NewMethod(node);
					CilInstructionCollection instructions = method.GetInstructions();

					CilInstructionLabel returnLabel = new();
					CilInstructionLabel exitLabel = new();

					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Callvirt, enterListMethod.MakeGenericInstanceMethod(listNode.Child.TypeSignature));
					instructions.Add(CilOpCodes.Brfalse, returnLabel);

					{
						IMethodDescriptor elementMethod = GetOrMakeMethod(listNode.Child);

						//Make local and store length in it
						CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
						instructions.Add(CilOpCodes.Ldarg_0); //Load list
						instructions.Add(CilOpCodes.Call, listNode.GetCount); //Get count
						instructions.Add(CilOpCodes.Stloc, countLocal); //Store it

						//Avoid the loop if count is less than 1
						instructions.Add(CilOpCodes.Ldloc, countLocal);
						instructions.Add(CilOpCodes.Ldc_I4_1);
						instructions.Add(CilOpCodes.Blt, exitLabel);

						//Make an i
						CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
						instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
						instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

						//Jump over dividing for i == 0
						CilInstructionLabel visitItemLabel = new();
						instructions.Add(CilOpCodes.Br, visitItemLabel);

						//Divide List
						ICilLabel loopStartLabel = instructions.Add(CilOpCodes.Nop).CreateLabel();
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Callvirt, divideListMethod.MakeGenericInstanceMethod([.. listNode.TypeSignature.TypeArguments]));

						//Visit Item
						visitItemLabel.Instruction = instructions.Add(CilOpCodes.Nop);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Ldloc, iLocal);
						instructions.Add(CilOpCodes.Call, listNode.GetItem);
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.AddCall(elementMethod);

						//Increment i
						instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
						instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
						instructions.Add(CilOpCodes.Add); //Add 
						instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

						//Jump to start of loop if i < count
						instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
						instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
						instructions.Add(CilOpCodes.Blt, loopStartLabel); //Jump back up if less than
					}

					exitLabel.Instruction = instructions.Add(CilOpCodes.Nop);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Callvirt, exitListMethod.MakeGenericInstanceMethod(listNode.Child.TypeSignature));

					returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
					result = method;
				}
				break;
			case DictionaryNode dictionaryNode:
				{
					MethodDefinition method = NewMethod(node);
					CilInstructionCollection instructions = method.GetInstructions();

					CilInstructionLabel returnLabel = new();
					CilInstructionLabel exitLabel = new();

					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Callvirt, enterDictionaryMethod.MakeGenericInstanceMethod([.. dictionaryNode.TypeSignature.TypeArguments]));
					instructions.Add(CilOpCodes.Brfalse, returnLabel);

					{
						//Make local and store length in it
						CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32); //Create local
						instructions.Add(CilOpCodes.Ldarg_0); //Load collection
						instructions.Add(CilOpCodes.Call, dictionaryNode.GetCount); //Get count
						instructions.Add(CilOpCodes.Stloc, countLocal); //Store it

						//Avoid the loop if count is less than 1
						instructions.Add(CilOpCodes.Ldloc, countLocal);
						instructions.Add(CilOpCodes.Ldc_I4_1);
						instructions.Add(CilOpCodes.Blt, exitLabel);

						//Make an i
						CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
						instructions.Add(CilOpCodes.Ldc_I4_0); //Load 0 as an int32
						instructions.Add(CilOpCodes.Stloc, iLocal); //Store in count

						//Jump over dividing for i == 0
						CilInstructionLabel visitPairLabel = new();
						instructions.Add(CilOpCodes.Br, visitPairLabel);

						//Divide Dictionary
						ICilLabel loopStartLabel = instructions.Add(CilOpCodes.Nop).CreateLabel();
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Callvirt, divideDictionaryMethod.MakeGenericInstanceMethod([.. dictionaryNode.TypeSignature.TypeArguments]));

						//Visit Pair
						{
							PairNode pairNode = dictionaryNode.Child;

							IMethodDescriptor keyMethod = GetOrMakeMethod(pairNode.Key);
							IMethodDescriptor valueMethod = GetOrMakeMethod(pairNode.Value);

							visitPairLabel.Instruction = instructions.Add(CilOpCodes.Nop);

							CilLocalVariable pairLocal = instructions.AddLocalVariable(pairNode.TypeSignature);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldloc, iLocal);
							instructions.Add(CilOpCodes.Call, dictionaryNode.GetPair);
							instructions.Add(CilOpCodes.Stloc, pairLocal);

							CilInstructionLabel afterPairLabel = new();
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Ldloc, pairLocal);
							instructions.Add(CilOpCodes.Call, pairNode.ImplicitConversion);
							instructions.Add(CilOpCodes.Callvirt, enterDictionaryPairMethod.MakeGenericInstanceMethod([.. pairNode.TypeSignature.TypeArguments]));
							instructions.Add(CilOpCodes.Brfalse, afterPairLabel);

							instructions.Add(CilOpCodes.Ldloc, pairLocal);
							instructions.Add(CilOpCodes.Call, pairNode.GetKey);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.AddCall(keyMethod);

							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Ldloc, pairLocal);
							instructions.Add(CilOpCodes.Call, pairNode.ImplicitConversion);
							instructions.Add(CilOpCodes.Callvirt, divideDictionaryPairMethod.MakeGenericInstanceMethod([.. pairNode.TypeSignature.TypeArguments]));

							instructions.Add(CilOpCodes.Ldloc, pairLocal);
							instructions.Add(CilOpCodes.Call, pairNode.GetValue);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.AddCall(valueMethod);

							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Ldloc, pairLocal);
							instructions.Add(CilOpCodes.Call, pairNode.ImplicitConversion);
							instructions.Add(CilOpCodes.Callvirt, exitDictionaryPairMethod.MakeGenericInstanceMethod([.. pairNode.TypeSignature.TypeArguments]));

							afterPairLabel.Instruction = instructions.Add(CilOpCodes.Nop);
						}

						//Increment i
						instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i local
						instructions.Add(CilOpCodes.Ldc_I4_1); //Load constant 1 as int32
						instructions.Add(CilOpCodes.Add); //Add 
						instructions.Add(CilOpCodes.Stloc, iLocal); //Store in i local

						//Jump to start of loop if i < count
						instructions.Add(CilOpCodes.Ldloc, iLocal); //Load i
						instructions.Add(CilOpCodes.Ldloc, countLocal); //Load count
						instructions.Add(CilOpCodes.Blt, loopStartLabel); //Jump back up if less than
					}

					exitLabel.Instruction = instructions.Add(CilOpCodes.Nop);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Callvirt, exitDictionaryMethod.MakeGenericInstanceMethod([.. dictionaryNode.TypeSignature.TypeArguments]));

					returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
					result = method;
				}
				break;
			case PairNode pairNode:
				{
					MethodDefinition method = NewMethod(node);
					CilInstructionCollection instructions = method.GetInstructions();

					CilInstructionLabel returnLabel = new();

					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Call, pairNode.ImplicitConversion);
					instructions.Add(CilOpCodes.Callvirt, enterPairMethod.MakeGenericInstanceMethod([.. pairNode.TypeSignature.TypeArguments]));
					instructions.Add(CilOpCodes.Brfalse, returnLabel);

					{
						IMethodDescriptor keyMethod = GetOrMakeMethod(pairNode.Key);
						IMethodDescriptor valueMethod = GetOrMakeMethod(pairNode.Value);

						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Call, pairNode.GetKey);
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.AddCall(keyMethod);

						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Call, pairNode.ImplicitConversion);
						instructions.Add(CilOpCodes.Callvirt, dividePairMethod.MakeGenericInstanceMethod([.. pairNode.TypeSignature.TypeArguments]));

						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Call, pairNode.GetValue);
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.AddCall(valueMethod);
					}

					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Call, pairNode.ImplicitConversion);
					instructions.Add(CilOpCodes.Callvirt, exitPairMethod.MakeGenericInstanceMethod([.. pairNode.TypeSignature.TypeArguments]));

					returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
					result = method;
				}
				break;
			case KeyNode keyNode:
				return GetOrMakeMethod(keyNode.Child);
			case ValueNode valueNode:
				return GetOrMakeMethod(valueNode.Child);
			default:
				throw new NotSupportedException();
		}
		MethodDictionary.Add(node.TypeSignature, result);
		return result;

		static MethodDefinition NewMethod(Node node)
		{
			MethodDefinition method = helperClass.AddMethod(
				UniqueNameFactory.MakeUniqueName(node.TypeSignature),
				MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
				SharedState.Instance.Importer.Void);
			method.AddParameter(node.TypeSignature, "value");
			method.AddParameter(assetWalkerType, "walker");
			return method;
		}
	}

	private static void Initialize()
	{
		assetWalkerType = SharedState.Instance.Importer.ImportType<AssetWalker>().ToTypeSignature();

		enterAssetMethod = ImportWalkerMethod(nameof(AssetWalker.EnterAsset));
		divideAssetMethod = ImportWalkerMethod(nameof(AssetWalker.DivideAsset));
		exitAssetMethod = ImportWalkerMethod(nameof(AssetWalker.ExitAsset));

		enterFieldMethod = ImportWalkerMethod(nameof(AssetWalker.EnterField));
		exitFieldMethod = ImportWalkerMethod(nameof(AssetWalker.ExitField));

		enterListMethod = ImportWalkerMethod(nameof(AssetWalker.EnterList));
		divideListMethod = ImportWalkerMethod(nameof(AssetWalker.DivideList));
		exitListMethod = ImportWalkerMethod(nameof(AssetWalker.ExitList));

		enterDictionaryMethod = ImportWalkerMethod(nameof(AssetWalker.EnterDictionary));
		divideDictionaryMethod = ImportWalkerMethod(nameof(AssetWalker.DivideDictionary));
		exitDictionaryMethod = ImportWalkerMethod(nameof(AssetWalker.ExitDictionary));

		enterDictionaryPairMethod = ImportWalkerMethod(nameof(AssetWalker.EnterDictionaryPair));
		divideDictionaryPairMethod = ImportWalkerMethod(nameof(AssetWalker.DivideDictionaryPair));
		exitDictionaryPairMethod = ImportWalkerMethod(nameof(AssetWalker.ExitDictionaryPair));

		enterPairMethod = ImportWalkerMethod(nameof(AssetWalker.EnterPair));
		dividePairMethod = ImportWalkerMethod(nameof(AssetWalker.DividePair));
		exitPairMethod = ImportWalkerMethod(nameof(AssetWalker.ExitPair));

		visitPrimitiveMethod = ImportWalkerMethod(nameof(AssetWalker.VisitPrimitive));
		visitPPtrMethod = ImportWalkerMethod(nameof(AssetWalker.VisitPPtr));
	}

	private static void CreateEmptyMethods()
	{
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (TypeDefinition type in group.Types)
			{
				MethodDictionary.Add(type.ToTypeSignature(), AddMethod(type, MethodName, assetWalkerType));
			}
		}

		static MethodDefinition AddMethod(TypeDefinition type, string methodName, TypeSignature assetWalkerType)
		{
			MethodDefinition method = type.AddMethod(methodName, Pass063_CreateEmptyMethods.OverrideMethodAttributes, SharedState.Instance.Importer.Void);
			method.AddParameter(assetWalkerType, "walker");
			return method;
		}
	}

	private static IMethodDefOrRef ImportWalkerMethod(string methodName)
	{
		return SharedState.Instance.Importer.ImportMethod<AssetWalker>(m =>
		{
			if (m.Name != methodName)
			{
				return false;
			}
			else if (methodName == nameof(AssetWalker.VisitPPtr))
			{
				return !m.Parameters[0].ParameterType.IsValueType;
			}
			else if (methodName is nameof(AssetWalker.VisitPrimitive))
			{
				return true;
			}
			else
			{
				TypeSignature parameterType = m.Parameters[0].ParameterType;
				return parameterType is not GenericInstanceTypeSignature || !(parameterType.Namespace?.StartsWith("AssetRipper") ?? false);
			}
		});
	}

	private static CilInstruction AddCall(this CilInstructionCollection instructions, IMethodDescriptor method)
	{
		return method is MethodDefinition { IsStatic: true }
			? instructions.Add(CilOpCodes.Call, method)
			: instructions.Add(CilOpCodes.Callvirt, method);
	}
}