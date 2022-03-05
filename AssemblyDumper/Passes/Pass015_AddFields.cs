using AsmResolver.DotNet.Signatures;
using AssemblyDumper.Unity;
using AssemblyDumper.Utils;
using AssetRipper.Core.Attributes;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;

namespace AssemblyDumper.Passes
{
	public static class Pass015_AddFields
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static IMethodDefOrRef ReleaseOnlyAttributeConstructor { get; set; }
		private static IMethodDefOrRef EditorOnlyAttributeConstructor { get; set; }
		private static ITypeDefOrRef TransferMetaFlagsDefinition { get; set; }
		private static IMethodDefOrRef EditorMetaFlagsAttributeConstructor { get; set; }
		private static IMethodDefOrRef ReleaseMetaFlagsAttributeConstructor { get; set; }
		private static IMethodDefOrRef OriginalNameAttributeConstructor { get; set; }
		private static ITypeDefOrRef AssetDictionaryType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		private static void InitializeImports()
		{
			ReleaseOnlyAttributeConstructor = SharedState.Importer.ImportCommonConstructor<ReleaseOnlyAttribute>();
			EditorOnlyAttributeConstructor = SharedState.Importer.ImportCommonConstructor<EditorOnlyAttribute>();
			TransferMetaFlagsDefinition = SharedState.Importer.ImportCommonType<TransferMetaFlags>();
			EditorMetaFlagsAttributeConstructor = SharedState.Importer.ImportCommonConstructor<EditorMetaFlagsAttribute>(1);
			ReleaseMetaFlagsAttributeConstructor = SharedState.Importer.ImportCommonConstructor<ReleaseMetaFlagsAttribute>(1);
			OriginalNameAttributeConstructor = SharedState.Importer.ImportCommonConstructor<OriginalNameAttribute>(1);
			AssetDictionaryType = SharedState.Importer.ImportCommonType("AssetRipper.Core.IO.AssetDictionary`2");
		}

		public static void DoPass()
		{
			Console.WriteLine("Pass 015: Add Fields");

			InitializeImports();

			foreach ((string name, UnityClass unityClass) in SharedState.ClassDictionary)
			{
				ProcessNodeInformation(name, unityClass);
			}
		}

		private static void ProcessNodeInformation(string name, UnityClass unityClass)
		{
			TypeDefinition type = SharedState.TypeDictionary[name];

			if (unityClass.EditorRootNode == null && unityClass.ReleaseRootNode == null)
				return; //No fields.

			GetFieldNodeSets(unityClass, out List<UnityNode> releaseOnly, out List<UnityNode> editorOnly, out List<(UnityNode, UnityNode)> releaseAndEditor);

			foreach(UnityNode releaseOnlyField in releaseOnly)
			{
				TypeSignature releaseOnlyFieldType = ResolveFieldType(releaseOnlyField);
				type.AddReleaseOnlyField(releaseOnlyField, releaseOnlyFieldType);
			}

			foreach (UnityNode editorOnlyField in editorOnly)
			{
				TypeSignature editorOnlyFieldType = ResolveFieldType(editorOnlyField);
				type.AddEditorOnlyField(editorOnlyField, editorOnlyFieldType);
			}

			foreach ((UnityNode releaseField, UnityNode editorField) in releaseAndEditor)
			{
				TypeSignature fieldType = ResolveFieldType(releaseField);
				type.AddNormalField(releaseField, editorField, fieldType);
			}
		}

		private static void GetFieldNodeSets(UnityClass unityClass, out List<UnityNode> releaseOnly, out List<UnityNode> editorOnly, out List<(UnityNode, UnityNode)> releaseAndEditor)
		{
			List<UnityNode> editorNodes = unityClass.GetNonInheritedEditorNodes();
			List<UnityNode> releaseNodes = unityClass.GetNonInheritedReleaseNodes();

			Dictionary<string, UnityNode> releaseFields = releaseNodes.ToDictionary(x => x.Name, x => x);
			Dictionary<string, UnityNode> editorFields = editorNodes.ToDictionary(x => x.Name, x => x);

			List<UnityNode> releaseOnlyResult = releaseNodes.Where(node => !editorFields.ContainsKey(node.Name)).ToList();
			//Need to use a result local field here becuase out parameters can't be used in lambda expressions
			editorOnly = editorNodes.Where(node => !releaseFields.ContainsKey(node.Name)).ToList();

			releaseAndEditor = releaseNodes.
				Where(anyRelease => !releaseOnlyResult.Contains(anyRelease)).
				Select(releaseWithEditor => (releaseWithEditor, editorFields[releaseWithEditor.Name])).
				ToList();

			releaseOnly = releaseOnlyResult;
		}

		private static List<UnityNode> GetNonInheritedEditorNodes(this UnityClass unityClass)
		{
			List<UnityNode> editorNodes = unityClass.EditorRootNode?.SubNodes ?? new();
			return editorNodes.Where(node => !IsFieldInBaseType(unityClass, node.Name)).ToList();
		}

		private static List<UnityNode> GetNonInheritedReleaseNodes(this UnityClass unityClass)
		{
			List<UnityNode> releaseNodes = unityClass.ReleaseRootNode?.SubNodes ?? new();
			return releaseNodes.Where(node => !IsFieldInBaseType(unityClass, node.Name)).ToList();
		}

		private static void AddReleaseOnlyField(this TypeDefinition type, UnityNode releaseNode, TypeSignature fieldType)
		{
			FieldSignature? fieldSignature = FieldSignature.CreateInstance(fieldType);
			FieldDefinition fieldDefinition = new FieldDefinition(releaseNode.Name, FieldAttributes.Public, fieldSignature);
			fieldDefinition.MaybeAddOriginalNameAttribute(releaseNode, null);
			fieldDefinition.AddReleaseFlagAttribute(releaseNode.MetaFlag);
			fieldDefinition.AddCustomAttribute(ReleaseOnlyAttributeConstructor);
			type.Fields.Add(fieldDefinition);
		}

		private static void AddEditorOnlyField(this TypeDefinition type, UnityNode editorNode, TypeSignature fieldType)
		{
			FieldSignature? fieldSignature = FieldSignature.CreateInstance(fieldType);
			FieldDefinition fieldDefinition = new FieldDefinition(editorNode.Name, FieldAttributes.Public, fieldSignature);
			fieldDefinition.MaybeAddOriginalNameAttribute(null, editorNode);
			fieldDefinition.AddCustomAttribute(EditorOnlyAttributeConstructor);
			fieldDefinition.AddEditorFlagAttribute(editorNode.MetaFlag);
			type.Fields.Add(fieldDefinition);
		}

		private static void AddNormalField(this TypeDefinition type, UnityNode releaseNode, UnityNode editorNode, TypeSignature fieldType)
		{
			FieldSignature? fieldSignature = FieldSignature.CreateInstance(fieldType);
			FieldDefinition fieldDefinition = new FieldDefinition(editorNode.Name, FieldAttributes.Public, fieldSignature);
			fieldDefinition.MaybeAddOriginalNameAttribute(releaseNode, editorNode);
			fieldDefinition.AddReleaseFlagAttribute(releaseNode.MetaFlag);
			fieldDefinition.AddEditorFlagAttribute(editorNode.MetaFlag);
			type.Fields.Add(fieldDefinition);
		}

		private static void MaybeAddOriginalNameAttribute(this FieldDefinition field, UnityNode? releaseNode, UnityNode? editorNode)
		{
			if(releaseNode == null)
			{
				if (editorNode == null)
					throw new Exception("Release and editor nodes can't both be null");

				if(editorNode.Name != editorNode.OriginalName)
				{
					field.AddOriginalNameAttribute(editorNode.OriginalName);
				}
			}
			else if (editorNode == null)
			{
				if (releaseNode.Name != releaseNode.OriginalName)
				{
					field.AddOriginalNameAttribute(releaseNode.OriginalName);
				}
			}
			else
			{
				Assertions.AssertEquality(releaseNode.Name, editorNode.Name);
				Assertions.AssertEquality(releaseNode.OriginalName, editorNode.OriginalName);
				if (releaseNode.Name != releaseNode.OriginalName)
				{
					field.AddOriginalNameAttribute(releaseNode.OriginalName);
				}
			}
		}

		private static TypeSignature ResolveFieldType(UnityNode editorField)
		{
			TypeSignature? fieldType = SystemTypeGetter.GetCppPrimitiveTypeSignature(editorField.TypeName);

			if (fieldType == null && SharedState.TypeDictionary.TryGetValue(editorField.TypeName, out TypeDefinition? result))
				fieldType = result.ToTypeSignature();

			if (fieldType == null)
			{
				switch (editorField.TypeName)
				{
					case "vector":
					case "set":
					case "staticvector":
						UnityNode arrayNode = editorField.SubNodes[0];
						return ResolveArrayType(arrayNode);
					case "map":
						return ResolveDictionaryType(editorField);
					case "pair":
						return ResolvePairType(editorField);
					case "TypelessData":
						return SystemTypeGetter.UInt8.MakeSzArrayType();
					case "Array":
						return ResolveArrayType(editorField);
				}
			}

			if (fieldType == null)
			{
				throw new Exception($"Could not resolve field type {editorField.TypeName}");
			}

			return fieldType;
		}

		private static GenericInstanceTypeSignature ResolveDictionaryType(UnityNode dictionaryNode)
		{
			UnityNode pairNode = dictionaryNode.SubNodes[0].SubNodes[1];
			ResolvePairElementTypes(pairNode, out TypeSignature firstType, out TypeSignature secondType);
			return AssetDictionaryType.MakeGenericInstanceType(firstType, secondType);
		}

		private static GenericInstanceTypeSignature ResolvePairType(UnityNode pairNode)
		{
			ResolvePairElementTypes(pairNode, out TypeSignature firstType, out TypeSignature secondType);
			ITypeDefOrRef kvpType = CommonTypeGetter.NullableKeyValuePair;
			return kvpType.MakeGenericInstanceType(firstType, secondType);
		}

		private static void ResolvePairElementTypes(UnityNode pairNode, out TypeSignature firstType, out TypeSignature secondType)
		{
			firstType = ResolveFieldType(pairNode.SubNodes[0]);
			secondType = ResolveFieldType(pairNode.SubNodes[1]);

			if (firstType == null || secondType == null)
			{
				throw new Exception($"Could not resolve one of the parameters in a pair: first is {pairNode.SubNodes[0].TypeName}, second is {pairNode.SubNodes[1].TypeName}");
			}
		}

		private static SzArrayTypeSignature ResolveArrayType(UnityNode arrayNode)
		{
			UnityNode arrayTypeNode = arrayNode.SubNodes[1];
			TypeSignature arrayType = ResolveFieldType(arrayTypeNode);

			if (arrayType == null)
			{
				throw new Exception($"Could not resolve array parameter {arrayTypeNode.TypeName}");
			}

			return arrayType.MakeAndImportArrayType();
		}

		private static void AddReleaseFlagAttribute(this FieldDefinition _this, uint flags)
		{
			_this.AddCustomAttribute(ReleaseMetaFlagsAttributeConstructor, TransferMetaFlagsDefinition.ToTypeSignature(), flags);
		}

		private static void AddEditorFlagAttribute(this FieldDefinition _this, uint flags)
		{
			_this.AddCustomAttribute(EditorMetaFlagsAttributeConstructor, TransferMetaFlagsDefinition.ToTypeSignature(), flags);
		}

		private static void AddOriginalNameAttribute(this FieldDefinition _this, string originalName)
		{
			_this.AddCustomAttribute(OriginalNameAttributeConstructor, SystemTypeGetter.String, originalName);
		}

		private static bool IsFieldInBaseType(UnityClass unityClass, string fieldName)
		{
			string baseTypeName = unityClass.Base;
			while (!string.IsNullOrEmpty(baseTypeName))
			{
				UnityClass baseType = SharedState.ClassDictionary[baseTypeName];

				if (baseType.EditorRootNode?.SubNodes.Any(n => n.Name == fieldName) == true)
					return true;

				if (baseType.ReleaseRootNode?.SubNodes.Any(n => n.Name == fieldName) == true)
					return true;

				baseTypeName = baseType.Base;
			}

			return false;
		}
	}
}