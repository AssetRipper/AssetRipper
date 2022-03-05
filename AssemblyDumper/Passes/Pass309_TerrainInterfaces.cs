using AssemblyDumper.Utils;
using AssetRipper.Core.Classes.Terrain;
using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Math.Vectors;

namespace AssemblyDumper.Passes
{
	public static class Pass309_TerrainInterfaces
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 309: Terrain Interfaces");
			if (SharedState.TypeDictionary.TryGetValue("Heightmap", out TypeDefinition? heightmap))
			{
				heightmap.ImplementHeightmap();
			}
			if (SharedState.TypeDictionary.TryGetValue("TerrainData", out TypeDefinition?terrainData))
			{
				terrainData.ImplementTerrainData();
			}
			if (SharedState.TypeDictionary.TryGetValue("Terrain", out TypeDefinition? terrain))
			{
				terrain.ImplementTerrain();
			}
		}

		private static void ImplementHeightmap(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IHeightmap>();

			if(type.TryGetFieldByName("m_Resolution", out FieldDefinition? resolutionField))
			{
				type.ImplementFullProperty(nameof(IHeightmap.Width), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, resolutionField);
				type.ImplementFullProperty(nameof(IHeightmap.Height), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, resolutionField);
			}
			else
			{
				type.ImplementFullProperty(nameof(IHeightmap.Width), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, type.GetFieldByName("m_Width"));
				type.ImplementFullProperty(nameof(IHeightmap.Height), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, type.GetFieldByName("m_Height"));
			}
			type.ImplementFullProperty(nameof(IHeightmap.Heights), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int16.MakeSzArrayType(), type.GetFieldByName("m_Heights"));

			FieldDefinition scaleField = type.GetFieldByName("m_Scale");
			MethodDefinition implicitConversion = scaleField.Signature!.FieldType.Resolve()!.Methods.Single(m => m.Name == "op_Implicit");
			PropertyDefinition scaleProperty = type.AddGetterProperty(
				nameof(IHeightmap.Scale),
				InterfaceUtils.InterfacePropertyImplementation,
				SharedState.Importer.ImportCommonType<Vector3f>().ToTypeSignature());
			CilInstructionCollection processor = scaleProperty.GetMethod!.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, scaleField);
			processor.Add(CilOpCodes.Call, implicitConversion);
			processor.Add(CilOpCodes.Ret);
		}

		private static void ImplementTerrainData(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<ITerrainData>();
			type.ImplementGetterProperty(
				nameof(ITerrainData.Heightmap),
				InterfaceUtils.InterfacePropertyImplementation,
				SharedState.Importer.ImportCommonType<IHeightmap>().ToTypeSignature(),
				type.GetFieldByName("m_Heightmap"));
		}

		private static void ImplementTerrain(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<ITerrain>();
			FieldDefinition field = type.GetFieldByName("m_TerrainData");
			TypeDefinition fieldType = field.Signature!.FieldType.Resolve()!;
			MethodDefinition explicitConversion = PPtrUtils.GetExplicitConversion<ITerrainData>(fieldType);
			TypeSignature returnType = explicitConversion.Signature!.ReturnType;
			type.ImplementGetterProperty(nameof(ITerrain.TerrainData), InterfaceUtils.InterfacePropertyImplementation, returnType, field);
		}
	}
}
