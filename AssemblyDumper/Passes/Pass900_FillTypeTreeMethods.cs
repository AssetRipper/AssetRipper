using AssemblyDumper.Unity;
using AssemblyDumper.Utils;
using AssetRipper.Core;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;

namespace AssemblyDumper.Passes
{
	public static class Pass900_FillTypeTreeMethods
	{
		private const MethodAttributes OverrideMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot | MethodAttributes.HideBySig;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static ITypeDefOrRef typeTreeNode;
		private static IMethodDefOrRef typeTreeNodeConstructor;
		private static GenericInstanceTypeSignature typeTreeNodeList;
		private static IMethodDefOrRef typeTreeNodeListConstructor;
		private static IMethodDefOrRef listAddMethod;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static bool generateEmptyMethods = false;

		public static void DoPass()
		{
			System.Console.WriteLine("Pass 900: Fill Type Tree Methods");

			typeTreeNode = SharedState.Importer.ImportCommonType<TypeTreeNode>();
			typeTreeNodeConstructor = SharedState.Importer.ImportCommonConstructor<TypeTreeNode>(8);
			typeTreeNodeList = SystemTypeGetter.List.MakeGenericInstanceType(typeTreeNode.ToTypeSignature());
			typeTreeNodeListConstructor = MethodUtils.MakeConstructorOnGenericType(typeTreeNodeList, 0);
			listAddMethod = MethodUtils.MakeMethodOnGenericType(typeTreeNodeList, typeTreeNodeList.Resolve()!.Methods.First(m => m.Name == "Add"));

			foreach ((string name, UnityClass klass) in SharedState.ClassDictionary)
			{
				if (!SharedState.TypeDictionary.ContainsKey(name))
					//Skip primitive types
					continue;

				TypeDefinition type = SharedState.TypeDictionary[name];

				MethodDefinition editorModeMethod = type.AddMethod(nameof(UnityAssetBase.MakeEditorTypeTreeNodes), OverrideMethodAttributes, typeTreeNodeList);
				editorModeMethod.AddParameter("depth", SystemTypeGetter.Int32);
				editorModeMethod.AddParameter("startingIndex", SystemTypeGetter.Int32);

				MethodDefinition releaseModeMethod = type.AddMethod(nameof(UnityAssetBase.MakeReleaseTypeTreeNodes), OverrideMethodAttributes, typeTreeNodeList);
				releaseModeMethod.AddParameter("depth", SystemTypeGetter.Int32);
				releaseModeMethod.AddParameter("startingIndex", SystemTypeGetter.Int32);

				CilMethodBody editorModeBody = editorModeMethod.CilMethodBody!;
				CilMethodBody releaseModeBody = releaseModeMethod.CilMethodBody!;

				CilInstructionCollection editorModeProcessor = editorModeBody.Instructions;
				CilInstructionCollection releaseModeProcessor = releaseModeBody.Instructions;
				
				//Console.WriteLine($"Generating the editor read method for {name}");
				if (klass.EditorRootNode == null || generateEmptyMethods)
				{
					editorModeProcessor.AddNotSupportedException();
				}
				else
				{
					editorModeProcessor.AddTypeTreeCreation(klass.EditorRootNode);
				}

				//Console.WriteLine($"Generating the release read method for {name}");
				if (klass.ReleaseRootNode == null || generateEmptyMethods)
				{
					releaseModeProcessor.AddNotSupportedException();
				}
				else
				{
					releaseModeProcessor.AddTypeTreeCreation(klass.ReleaseRootNode);
				}

				editorModeProcessor.OptimizeMacros();
				releaseModeProcessor.OptimizeMacros();
			}
		}

		private static void AddTypeTreeCreation(this CilInstructionCollection processor, UnityNode rootNode)
		{
			processor.Add(CilOpCodes.Newobj, typeTreeNodeListConstructor);

			processor.AddTreeNodesRecursively(rootNode, 0);

			processor.Add(CilOpCodes.Ret);
		}

		/// <summary>
		/// Emits all the type tree nodes recursively
		/// </summary>
		/// <param name="processor">The IL processor emitting the code</param>
		/// <param name="listVariable">The local list variable that type tree nodes are added to</param>
		/// <param name="node">The Unity node being emitted as a type tree node</param>
		/// <param name="currentIndex">The index of the emitted tree node relative to the root node</param>
		/// <returns>The relative index of the next tree node to be emitted</returns>
		private static int AddTreeNodesRecursively(this CilInstructionCollection processor, UnityNode node, int currentIndex)
		{
			processor.AddSingleTreeNode(node, currentIndex);
			currentIndex++;
			foreach (UnityNode? subNode in node.SubNodes)
			{
				currentIndex = processor.AddTreeNodesRecursively(subNode, currentIndex);
			}
			return currentIndex;
		}

		private static void AddSingleTreeNode(this CilInstructionCollection processor, UnityNode node, int currentIndex)
		{
			//For the add method at the end
			processor.Add(CilOpCodes.Dup);

			processor.Add(CilOpCodes.Ldstr, node.OriginalTypeName);
			processor.Add(CilOpCodes.Ldstr, node.OriginalName);

			//Level
			processor.Add(CilOpCodes.Ldarg_1);//depth
			processor.Add(CilOpCodes.Ldc_I4, (int)node.Level);//Because of recalculation in Pass 4, the root node is always zero
			processor.Add(CilOpCodes.Add);

			processor.Add(CilOpCodes.Ldc_I4, node.ByteSize);

			//Index
			processor.Add(CilOpCodes.Ldarg_2);//starting index
			processor.Add(CilOpCodes.Ldc_I4, currentIndex);
			processor.Add(CilOpCodes.Add);

			processor.Add(CilOpCodes.Ldc_I4, node.Version);
			processor.Add(CilOpCodes.Ldc_I4, (int)node.TypeFlags);
			processor.Add(CilOpCodes.Ldc_I4, unchecked((int)node.MetaFlag));
			processor.Add(CilOpCodes.Newobj, typeTreeNodeConstructor);

			processor.Add(CilOpCodes.Call, listAddMethod);
		}
	}
}
