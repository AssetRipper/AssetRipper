using AssetRipper.Core.Configuration;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using Mono.Cecil;
using System.IO;
using System.Text;
using Cpp2IlApi = Cpp2IL.Core.Cpp2IlApi;

namespace AssetRipper.Core.Structure.Assembly.Managers
{
	public sealed class IL2CppManager : BaseManager
	{
		public string? GameAssemblyPath { get; private set; }
		public string? UnityPlayerPath { get; private set; }
		public string? GameDataPath { get; private set; }
		public string? MetaDataPath { get; private set; }
		public int[]? UnityVersion { get; private set; }
		private readonly ScriptContentLevel contentLevel;

		public IL2CppManager(LayoutInfo layout, Action<string> requestAssemblyCallback, ScriptContentLevel level) : base(layout, requestAssemblyCallback)
		{
			contentLevel = level;
		}

		public override ScriptingBackend ScriptingBackend => ScriptingBackend.IL2Cpp;

		public override void Initialize(PlatformGameStructure gameStructure)
		{
			string? gameDataPath = gameStructure.GameDataPath;
			if (string.IsNullOrWhiteSpace(gameDataPath))
			{
				throw new ArgumentNullException(nameof(gameDataPath));
			}

			GameDataPath = gameDataPath;
			GameAssemblyPath = gameStructure.Il2CppGameAssemblyPath;
			UnityPlayerPath = gameStructure.UnityPlayerPath;
			MetaDataPath = gameStructure.Il2CppMetaDataPath;

			if (gameStructure.UnityVersion != null)
			{
				UnityVersion = gameStructure.UnityVersion;
			}
			else
			{
				UnityVersion = Cpp2IlApi.DetermineUnityVersion(UnityPlayerPath!, GameDataPath);
			}

			if (UnityVersion == null)
			{
				throw new NullReferenceException("Could not determine the unity version");
			}
			else
			{
				Logger.Info(LogCategory.Import, $"During Il2Cpp initialization, found Unity version: {MakeVersionString(UnityVersion)}");
			}

			Logger.SendStatusChange("loading_step_parse_il2cpp_metadata");

			Cpp2IlApi.InitializeLibCpp2Il(GameAssemblyPath!, MetaDataPath!, UnityVersion, false);

			Logger.SendStatusChange("loading_step_generate_dummy_dll");

			Cpp2IlApi.MakeDummyDLLs(true);

			LibCpp2IL.InstructionSet instructionSet = LibCpp2IL.LibCpp2IlMain.Binary?.InstructionSet ?? throw new Exception("Null binary");
			Logger.Info(LogCategory.Import, $"During Il2Cpp initialization, found {instructionSet} instruction set.");

			Cpp2IL.Core.BaseKeyFunctionAddresses? keyFunctionAddresses = null;
			if (IsAddressScanSupported(instructionSet))
			{
				Logger.SendStatusChange("loading_step_locate_key_functions");
				keyFunctionAddresses = Cpp2IlApi.ScanForKeyFunctionAddresses();
			}

			if (IsAttributeRestorationSupported(instructionSet))
			{
				Logger.SendStatusChange("loading_step_restore_attributes");
				Cpp2IlApi.RunAttributeRestorationForAllAssemblies(keyFunctionAddresses);
			}

			if (contentLevel >= ScriptContentLevel.Level3)
			{
				bool unsafeAnalysis = contentLevel == ScriptContentLevel.Level4;
				foreach (AssemblyDefinition assembly in Cpp2IlApi.GeneratedAssemblies)
				{
					Cpp2IlApi.AnalyseAssembly(Cpp2IL.Core.AnalysisLevel.IL_ONLY, assembly, keyFunctionAddresses!, null, true, unsafeAnalysis);
				}
			}

			foreach (AssemblyDefinition assembly in Cpp2IlApi.GeneratedAssemblies)
			{
				m_assemblies.Add(assembly.Name.Name, assembly);
			}
		}

		public override void Load(string filePath)
		{
			throw new NotSupportedException();
		}

		public override void Read(Stream stream, string fileName)
		{
			throw new NotSupportedException();
		}

		private static string MakeVersionString(int[] version)
		{
			if (version == null || version.Length == 0)
			{
				return "";
			}

			StringBuilder builder = new StringBuilder();
			builder.Append(version[0]);
			for (int i = 1; i < version.Length; i++)
			{
				builder.Append('.');
				builder.Append(version[i]);
			}
			return builder.ToString();
		}

		private static bool IsAddressScanSupported(LibCpp2IL.InstructionSet set) => set switch
		{
			LibCpp2IL.InstructionSet.X86_32 => true,
			LibCpp2IL.InstructionSet.X86_64 => true,
			LibCpp2IL.InstructionSet.ARM64 => true,
			_ => false,
		};

		private static bool IsAttributeRestorationSupported(LibCpp2IL.InstructionSet set) => set switch
		{
			LibCpp2IL.InstructionSet.X86_32 => true,
			LibCpp2IL.InstructionSet.X86_64 => true,
			LibCpp2IL.InstructionSet.WASM => true,

			//Arm32 and Arm64 were excluded before for performance reasons.
			//However, the community requested that they be enabled anyway.
			LibCpp2IL.InstructionSet.ARM32 => true,
			LibCpp2IL.InstructionSet.ARM64 => true,

			_ => false,
		};

		~IL2CppManager()
		{
			Dispose(false);
		}
	}
}
