using AssetRipper.Core;
using AssetRipper.Core.Classes;

namespace AssemblyDumper.Passes
{
	public static class Pass011_ApplyInheritance
	{
		public static void DoPass()
		{
			System.Console.WriteLine("Pass 011: Apply Inheritance");

			ITypeDefOrRef unityObjectBaseDefinition = SharedState.Importer.ImportCommonType<UnityObjectBase>();
			ITypeDefOrRef unityAssetBaseDefinition = SharedState.Importer.ImportCommonType<UnityAssetBase>();
			ITypeDefOrRef utf8StringBaseDefinition = SharedState.Importer.ImportCommonType<Utf8StringBase>();

			foreach (KeyValuePair<string, Unity.UnityClass> pair in SharedState.ClassDictionary)
			{
				if (PrimitiveTypes.primitives.Contains(pair.Key))
					continue;
				if (string.IsNullOrEmpty(pair.Value.Base))
				{
					if (pair.Key == "Object")
					{
						SharedState.TypeDictionary[pair.Key].BaseType = unityObjectBaseDefinition;
					}
					else if (pair.Key == Pass002_RenameSubnodes.Utf8StringName)
					{
						SharedState.TypeDictionary[pair.Key].BaseType = utf8StringBaseDefinition;
					}
					else
					{
						SharedState.TypeDictionary[pair.Key].BaseType = unityAssetBaseDefinition;
					}
				}
				else
				{
					SharedState.TypeDictionary[pair.Key].BaseType = SharedState.TypeDictionary[pair.Value.Base];
				}
			}
		}
	}
}