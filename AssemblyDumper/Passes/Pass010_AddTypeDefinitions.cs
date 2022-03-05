using AssemblyDumper.Unity;
using AssemblyDumper.Utils;
using AssetRipper.Core.Attributes;

namespace AssemblyDumper.Passes
{
	public static class Pass010_AddTypeDefinitions
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static IMethodDefOrRef EditorOnlyAttributeConstructor { get; set; }
		private static IMethodDefOrRef ReleaseOnlyAttributeConstructor { get; set; }
		private static IMethodDefOrRef StrippedAttributeConstructor { get; set; }
		private static IMethodDefOrRef PersistentIDAttributeConstructor { get; set; }
		private static IMethodDefOrRef OriginalNameAttributeConstructor { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static void DoPass()
		{
			Console.WriteLine("Pass 010: Add Type Definitions");

			EditorOnlyAttributeConstructor = SharedState.Importer.ImportCommonConstructor<EditorOnlyAttribute>();
			ReleaseOnlyAttributeConstructor = SharedState.Importer.ImportCommonConstructor<ReleaseOnlyAttribute>();
			StrippedAttributeConstructor = SharedState.Importer.ImportCommonConstructor<StrippedAttribute>();
			PersistentIDAttributeConstructor = SharedState.Importer.ImportCommonConstructor<PersistentIDAttribute>(1);
			OriginalNameAttributeConstructor = SharedState.Importer.ImportCommonConstructor<OriginalNameAttribute>(1);

			AssemblyDefinition? assembly = SharedState.Assembly;
			foreach (KeyValuePair<string, UnityClass> pair in SharedState.ClassDictionary)
			{
				TypeDefinition? typeDef = assembly.CreateType(pair.Value);
				if (typeDef != null)
					SharedState.TypeDictionary.Add(pair.Key, typeDef);
			}
		}

		private static TypeDefinition? CreateType(this AssemblyDefinition _this, UnityClass @class)
		{
			string name = @class.Name;
			if (SystemTypeGetter.primitiveNamesCsharp.Contains(name))
				return null;
			TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

			if (@class.IsAbstract)
				typeAttributes |= TypeAttributes.Abstract;
			else if (@class.DescendantCount == 1) 
				typeAttributes |= TypeAttributes.Sealed;

			TypeDefinition typeDef = new TypeDefinition(GetNamespace(name), name, typeAttributes);

			if(@class.GetOriginalTypeName(out string originalTypeName))
			{
				typeDef.AddCustomAttribute(OriginalNameAttributeConstructor, SystemTypeGetter.String, originalTypeName);
			}

			if (@class.IsEditorOnly) typeDef.AddCustomAttribute(EditorOnlyAttributeConstructor);
			if (@class.IsReleaseOnly) typeDef.AddCustomAttribute(ReleaseOnlyAttributeConstructor);
			if (@class.IsStripped) typeDef.AddCustomAttribute(StrippedAttributeConstructor);
			typeDef.AddCustomAttribute(PersistentIDAttributeConstructor, SystemTypeGetter.Int32, @class.TypeID);

			_this.ManifestModule!.TopLevelTypes.Add(typeDef);

			return typeDef;
		}

		private static string GetNamespace(string className)
		{
			if(className.StartsWith("PPtr_", StringComparison.Ordinal))
			{
				return SharedState.ClassesNamespace + ".PPtrs";
			}
			else if (className.StartsWith("OffsetPtr_", StringComparison.Ordinal))
			{
				return SharedState.ClassesNamespace + ".OffsetPtrs";
			}
			else if (className.StartsWith("TestObject", StringComparison.Ordinal))
			{
				return SharedState.ClassesNamespace + ".TestObjects";
			}
			else
			{
				return SharedState.ClassesNamespace;
			}
		}
	}
}