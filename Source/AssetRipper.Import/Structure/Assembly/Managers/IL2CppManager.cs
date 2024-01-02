using AsmResolver.DotNet;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using Cpp2IL.Core.Api;
using Cpp2IL.Core.InstructionSets;
using Cpp2IL.Core.Model.Contexts;
using Cpp2IL.Core.OutputFormats;
using Cpp2IL.Core.ProcessingLayers;
using LibCpp2IL;
using Cpp2IlApi = Cpp2IL.Core.Cpp2IlApi;

namespace AssetRipper.Import.Structure.Assembly.Managers
{
	public sealed class IL2CppManager : BaseManager
	{
		static IL2CppManager()
		{
			InstructionSetRegistry.RegisterInstructionSet<X86InstructionSet>(DefaultInstructionSets.X86_32);
			InstructionSetRegistry.RegisterInstructionSet<X86InstructionSet>(DefaultInstructionSets.X86_64);
			InstructionSetRegistry.RegisterInstructionSet<WasmInstructionSet>(DefaultInstructionSets.WASM);
			InstructionSetRegistry.RegisterInstructionSet<ArmV7InstructionSet>(DefaultInstructionSets.ARM_V7);
			bool useNewArm64 = false;
			if (useNewArm64)
			{
				InstructionSetRegistry.RegisterInstructionSet<NewArmV8InstructionSet>(DefaultInstructionSets.ARM_V8);
			}
			else
			{
				InstructionSetRegistry.RegisterInstructionSet<Arm64InstructionSet>(DefaultInstructionSets.ARM_V8);
			}

			LibCpp2IlBinaryRegistry.RegisterBuiltInBinarySupport();
		}

		public string? GameAssemblyPath { get; private set; }
		public string? UnityPlayerPath { get; private set; }
		public string? GameDataPath { get; private set; }
		public string? MetaDataPath { get; private set; }
		public UnityVersion UnityVersion { get; private set; }
		/// <summary>
		/// For when analysis is reimplimented in Cpp2IL.
		/// </summary>
		private readonly ScriptContentLevel contentLevel;

		public IL2CppManager(Action<string> requestAssemblyCallback, ScriptContentLevel level) : base(requestAssemblyCallback)
		{
			contentLevel = level;
		}

		public override ScriptingBackend ScriptingBackend => ScriptingBackend.IL2Cpp;

		public override void Initialize(PlatformGameStructure gameStructure)
		{
			string? gameDataPath = gameStructure.GameDataPath;
			if (string.IsNullOrWhiteSpace(gameDataPath))
			{
				throw new ArgumentException($"{nameof(gameStructure.GameDataPath)} cannot be null or whitespace.", nameof(gameStructure));
			}

			GameDataPath = gameDataPath;
			GameAssemblyPath = gameStructure.Il2CppGameAssemblyPath;
			UnityPlayerPath = gameStructure.UnityPlayerPath;
			MetaDataPath = gameStructure.Il2CppMetaDataPath;

			UnityVersion = gameStructure.Version ?? Cpp2IlApi.DetermineUnityVersion(UnityPlayerPath, GameDataPath);

			if (UnityVersion == default)
			{
				throw new Exception("Could not determine the unity version");
			}
			else
			{
				Logger.Info(LogCategory.Import, $"During Il2Cpp initialization, found Unity version: {UnityVersion}");
			}

			Logger.SendStatusChange("loading_step_parse_il2cpp_metadata");

			Cpp2IlApi.InitializeLibCpp2Il(GameAssemblyPath!, MetaDataPath!, UnityVersion, false);

			Logger.SendStatusChange("loading_step_generate_dummy_dll");

			List<Cpp2IlProcessingLayer> processingLayers = new()
			{
				new AttributeAnalysisProcessingLayer(),
			};

			foreach (Cpp2IlProcessingLayer cpp2IlProcessingLayer in processingLayers)
			{
				cpp2IlProcessingLayer.PreProcess(GetCurrentAppContext(), processingLayers);
			}

			foreach (Cpp2IlProcessingLayer cpp2IlProcessingLayer in processingLayers)
			{
				cpp2IlProcessingLayer.Process(GetCurrentAppContext());
			}

			List<AssemblyDefinition> assemblies = new AsmResolverDllOutputFormatDefault().BuildAssemblies(GetCurrentAppContext());

			foreach (AssemblyDefinition assembly in assemblies)
			{
				assembly.InitializeResolvers(this);
				m_assemblies.Add(assembly.Name ?? throw new NullReferenceException(), assembly);
			}
		}

		private static ApplicationAnalysisContext GetCurrentAppContext()
		{
			return Cpp2IlApi.CurrentAppContext ?? throw new NullReferenceException();
		}

		public override void Load(string filePath)
		{
			throw new NotSupportedException();
		}

		public override void Read(Stream stream, string fileName)
		{
			throw new NotSupportedException();
		}

		~IL2CppManager()
		{
			Dispose(false);
		}
	}
}
