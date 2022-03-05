namespace AssemblyDumper.Passes
{
	public static class Pass360_AddMarkerInterfaces
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 360: Add Marker Interfaces");
			TryImplementInterface<AssetRipper.Core.Classes.INamedObject>("NamedObject");
			TryImplementInterface<AssetRipper.Core.Classes.IGameManager>("GameManager");
			TryImplementInterface<AssetRipper.Core.Classes.IGlobalGameManager>("GlobalGameManager");
			TryImplementInterface<AssetRipper.Core.Classes.ILevelGameManager>("LevelGameManager");
			TryImplementInterface<AssetRipper.Core.Classes.ILightmapParameters>("LightmapParameters");
			TryImplementInterface<AssetRipper.Core.Classes.IOcclusionPortal>("OcclusionPortal");
			TryImplementInterface<AssetRipper.Core.Classes.Renderer.IRenderer>("Renderer");
			TryImplementInterface<AssetRipper.Core.Classes.Meta.Importers.Asset.IAssetImporter>("AssetImporter");
			TryImplementInterface<AssetRipper.Core.Classes.Meta.Importers.IDefaultImporter>("DefaultImporter");
		}

		private static bool TryImplementInterface<T>(string typeName)
		{
			if (SharedState.TypeDictionary.TryGetValue(typeName, out TypeDefinition? type))
			{
				type.AddInterface<T>();
				return true;
			}
			return false;
		}

		private static void AddInterface<T>(this TypeDefinition type)
		{
			ITypeDefOrRef @interface = SharedState.Importer.ImportCommonType<T>();
			type.Interfaces.Add(new InterfaceImplementation(@interface));
		}
	}
}
