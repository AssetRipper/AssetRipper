using AssemblyDumper.Passes;
using CommandLine;
using System;
using System.IO;

namespace AssemblyDumper
{
	public static class Program
	{
		private static void Run(Options options)
		{
			Console.WriteLine("Making a new dll");
#if DEBUG
			try
			{
#endif
				Pass000_Initialize.DoPass(options.JsonPath!.FullName, options.SystemRuntimeAssembly!.FullName, options.SystemCollectionsAssembly!.FullName);
				Pass001_CreateBasicTypes.DoPass();
				Pass002_RenameSubnodes.DoPass();
				Pass003_ClassSpecificChanges.DoPass();
				Pass004_ExtractDependentNodeTrees.DoPass();
				Pass005_UnifyFieldsOfAbstractTypes.DoPass();
				//After this point, class dictionary does not change

				Pass010_AddTypeDefinitions.DoPass();
				Pass011_ApplyInheritance.DoPass();
				Pass015_AddFields.DoPass();

				Pass016_AddConstructors.DoPass();
				Pass017_FillConstructors.DoPass();

				Pass030_AddArrayInitializationMethods.DoPass();

				Pass080_PPtrConversions.DoPass();

				Pass099_CreateEmptyMethods.DoPass();
				Pass100_FillReadMethods.DoPass();
				Pass101_FillWriteMethods.DoPass();
				Pass102_FillYamlMethods.DoPass();
				Pass103_FillDependencyMethods.DoPass();

				Pass201_GuidImplicitConversion.DoPass();
				Pass202_VectorImplicitConversions.DoPass();
				Pass203_OffsetPtrImplicitConversions.DoPass();
				Pass204_Hash128ImplicitConversion.DoPass();

				Pass205_ObjectAndEditorExtension.DoPass();

				Pass300_ImplementHasNameInterface.DoPass();
				Pass301_ComponentInterface.DoPass();
				Pass302_MonoScriptInterface.DoPass();
				Pass303_BehaviourInterface.DoPass();
				Pass304_GameObjectInterface.DoPass();
				Pass305_TransformInterface.DoPass();
				Pass306_PrefabInstanceInterface.DoPass();

				Pass309_TerrainInterfaces.DoPass();
				Pass340_BuildSettingsInterfaces.DoPass();
				Pass341_ManagerInterfaces.DoPass();
				Pass342_AssetBundleInterfaces.DoPass();
				Pass343_SceneInterfaces.DoPass();

				Pass360_AddMarkerInterfaces.DoPass();
				Pass361_NativeImporterInterface.DoPass();
				Pass362_MiscellaneousExporters.DoPass();
				Pass363_ShaderInterfaces.DoPass();

				Pass500_FixPPtrYaml.DoPass();
				Pass501_MonoBehaviourImplementation.DoPass();
				Pass502_FixGuidAndHashYaml.DoPass();
				Pass503_FixUtf8String.DoPass();

				Pass900_FillTypeTreeMethods.DoPass();
				Pass940_MakeAssetFactory.DoPass();
				Pass942_MakeImporterFactory.DoPass();
				Pass950_UnityVersionHandler.DoPass();

				Pass998_ApplyAssemblyAttributes.DoPass();
				Pass999_SaveAssembly.DoPass(options.OutputDirectory!);
				Console.WriteLine("Done!");
#if DEBUG
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
#endif
		}

		internal class Options
		{
			[Value(0, Required = true, HelpText = "Information Json to parse")]
			public FileInfo? JsonPath { get; set; }

			[Option('o', "output", HelpText = "Directory to export to. Will not be cleared if already exists.")]
			public DirectoryInfo? OutputDirectory { get; set; }

			[Option("runtime", HelpText = "System.Runtime.dll from Net 6")]
			public FileInfo? SystemRuntimeAssembly { get; set; }

			[Option("collections", HelpText = "System.Collections.dll from Net 6")]
			public FileInfo? SystemCollectionsAssembly { get; set; }
		}

		public static void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<Options>(args)
				.WithParsed(options =>
				{
					if (ValidateOptions(options))
					{
						Run(options);
					}
					else
					{
						Environment.ExitCode = 1;
					}
				});
		}

		private static bool ValidateOptions(Options options)
		{
			try
			{
				if (options.JsonPath == null || !options.JsonPath.Exists)
					return false;
				if (options.SystemRuntimeAssembly == null)
					options.SystemRuntimeAssembly = new FileInfo(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.0\ref\net6.0\System.Runtime.dll");
				if (options.SystemCollectionsAssembly == null)
					options.SystemCollectionsAssembly = new FileInfo(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.0\ref\net6.0\System.Collections.dll");
				if (options.OutputDirectory == null)
					options.OutputDirectory = new DirectoryInfo(Environment.CurrentDirectory);

				return options.SystemRuntimeAssembly.Exists && options.SystemCollectionsAssembly.Exists;
			}
			catch (Exception ex)
			{
				System.Console.WriteLine($"Failed to initialize the paths.");
				System.Console.WriteLine(ex.ToString());
				return false;
			}
		}
	}
}
