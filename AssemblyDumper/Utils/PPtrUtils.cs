namespace AssemblyDumper.Utils
{
	public static class PPtrUtils
	{
		public static MethodDefinition GetExplicitConversion<T>(TypeDefinition pptrTypeDefinition)
		{
			//The pptr return types have a name like this
			//PPtr`1<AssetRipper.Core.Classes.GameObject.IGameObject>
			string returnTypeName = $"PPtr`1<{typeof(T).FullName}>";
			return pptrTypeDefinition.Methods.Single(m => m.Name == "op_Explicit" && m.Signature!.ReturnType.Name == returnTypeName);
		}
	}
}
