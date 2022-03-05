using AssemblyDumper.Unity;
using AssemblyDumper.Utils;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;

namespace AssemblyDumper.Passes
{
	public static class Pass101_FillWriteMethods
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static IMethodDefOrRef WriteMethod;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static bool throwNotSupported = true;

		public static void DoPass()
		{
			Console.WriteLine("Pass 101: Filling write methods");
			foreach ((string name, UnityClass klass) in SharedState.ClassDictionary)
			{
				TypeDefinition type = SharedState.TypeDictionary[name];
				List<FieldDefinition> fields = FieldUtils.GetAllFieldsInTypeAndBase(type);
				type.FillEditorWriteMethod(klass, fields);
				type.FillReleaseWriteMethod(klass, fields);
			}
		}

		private static void FillEditorWriteMethod(this TypeDefinition type, UnityClass klass, List<FieldDefinition> fields)
		{
			MethodDefinition editorModeReadMethod = type.Methods.First(m => m.Name == "WriteEditor");
			CilMethodBody editorModeBody = editorModeReadMethod.CilMethodBody!;
			CilInstructionCollection editorModeProcessor = editorModeBody.Instructions;
			
			if (throwNotSupported)
			{
				editorModeProcessor.AddNotSupportedException();
			}
			else
			{
				//Console.WriteLine($"Generating the editor read method for {name}");
				if (klass.EditorRootNode != null)
				{
					foreach (UnityNode unityNode in klass.EditorRootNode.SubNodes)
					{
						AddWriteToProcessor(unityNode, editorModeProcessor, fields);
					}
				}
				editorModeProcessor.Add(CilOpCodes.Ret);
			}
			editorModeProcessor.OptimizeMacros();
		}

		private static void FillReleaseWriteMethod(this TypeDefinition type, UnityClass klass, List<FieldDefinition> fields)
		{
			MethodDefinition releaseModeReadMethod = type.Methods.First(m => m.Name == "WriteRelease");
			CilMethodBody releaseModeBody = releaseModeReadMethod.CilMethodBody!;
			CilInstructionCollection releaseModeProcessor = releaseModeBody.Instructions;

			if (throwNotSupported)
			{
				releaseModeProcessor.AddNotSupportedException();
			}
			else
			{
				//Console.WriteLine($"Generating the release read method for {name}");
				if (klass.ReleaseRootNode != null)
				{
					foreach (UnityNode unityNode in klass.ReleaseRootNode.SubNodes)
					{
						AddWriteToProcessor(unityNode, releaseModeProcessor, fields);
					}
				}
				releaseModeProcessor.Add(CilOpCodes.Ret);
			}
			releaseModeProcessor.OptimizeMacros();
		}

		private static void AddWriteToProcessor(UnityNode node, CilInstructionCollection processor, List<FieldDefinition> fields)
		{
			//Get field
			FieldDefinition? field = fields.SingleOrDefault(f => f.Name == node.Name);

			if (field == null)
				throw new Exception($"Field {node.Name} cannot be found in {processor.Owner.Owner.DeclaringType} (fields are {string.Join(", ", fields.Select(f => f.Name))})");

			if (WriteMethod == null)
				WriteMethod = SharedState.Importer.ImportMethod(CommonTypeGetter.AssetWriterDefinition.Resolve()!.Methods.Single(m => m.Name == nameof(AssetWriter.WriteGeneric)));

			processor.Add(CilOpCodes.Ldarg_1); //Load writer
			processor.Add(CilOpCodes.Ldarg_0); //Load this
			processor.Add(CilOpCodes.Ldfld, field); //Load field

			MethodSpecification genericMethod = MethodUtils.MakeGenericInstanceMethod(WriteMethod, field.Signature!.FieldType);
			processor.Add(CilOpCodes.Call, genericMethod);

			MaybeAlignBytes(node, processor);
		}

		private static void MaybeAlignBytes(UnityNode node, CilInstructionCollection processor)
		{
			if (((TransferMetaFlags)node.MetaFlag).IsAlignBytes())
			{
				//Load reader
				processor.Add(CilOpCodes.Ldarg_1);

				//Get ReadAsset
				MethodDefinition alignMethod = CommonTypeGetter.EndianReaderDefinition.Resolve()!.Methods.First(m => m.Name == "AlignStream");

				//Call it
				processor.Add(CilOpCodes.Call, SharedState.Importer.ImportMethod(alignMethod));
			}
		}
	}
}