using AssemblyDumper.Unity;
using AssemblyDumper.Utils;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;

namespace AssemblyDumper.Passes
{
	public static class Pass016_AddConstructors
	{
		private const MethodAttributes PublicInstanceConstructorAttributes = 
			MethodAttributes.Public | 
			MethodAttributes.HideBySig | 
			MethodAttributes.SpecialName | 
			MethodAttributes.RuntimeSpecialName;
		private readonly static List<string> processed = new List<string>();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static ITypeDefOrRef AssetInfoRef;
		private static ITypeDefOrRef LayoutInfoRef;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static void DoPass()
		{
			System.Console.WriteLine("Pass 016: Add Constructors");
			AssetInfoRef = SharedState.Importer.ImportCommonType<AssetInfo>();
			LayoutInfoRef = SharedState.Importer.ImportCommonType<LayoutInfo>();
			foreach (KeyValuePair<string, UnityClass> pair in SharedState.ClassDictionary)
			{
				if (processed.Contains(pair.Key))
					continue;

				AddConstructor(pair.Value);
			}
		}

		private static void AddConstructor(UnityClass typeInfo)
		{
			if (PrimitiveTypes.primitives.Contains(typeInfo.Name))
				return;

			if (!string.IsNullOrEmpty(typeInfo.Base) && !processed.Contains(typeInfo.Base))
				AddConstructor(SharedState.ClassDictionary[typeInfo.Base]);

			TypeDefinition type = SharedState.TypeDictionary[typeInfo.Name];
			ConstructorUtils.AddDefaultConstructor(type);
			type.AddLayoutInfoConstructor();
			if(typeInfo.TypeID >= 0)
			{
				type.AddAssetInfoConstructor();
			}
			processed.Add(typeInfo.Name);
		}

		private static MethodDefinition AddAssetInfoConstructor(this TypeDefinition typeDefinition)
		{
			return AddSingleParameterConstructor(typeDefinition, AssetInfoRef, "info");
		}

		private static MethodDefinition AddLayoutInfoConstructor(this TypeDefinition typeDefinition)
		{
			return AddSingleParameterConstructor(typeDefinition, LayoutInfoRef, "info");
		}

		private static MethodDefinition AddSingleParameterConstructor(this TypeDefinition typeDefinition, ITypeDefOrRef parameterType, string parameterName)
		{
			MethodDefinition? constructor = typeDefinition.AddMethod(
				".ctor",
				PublicInstanceConstructorAttributes,
				SystemTypeGetter.Void
			);
			constructor.AddParameter(parameterName, parameterType);
			return constructor;
		}
	}
}