using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using System;
using System.IO;
using System.Text;
using Cpp2IlApi = Cpp2IL.Core.Cpp2IlApi;

namespace AssetRipper.Core.Structure.Assembly.Managers
{
	internal sealed class Il2CppManager : BaseManager
	{
		public string GameAssemblyPath { get; private set; }
		public string UnityPlayerPath { get; private set; }
		public string GameDataPath { get; private set; }
		public string MetaDataPath { get; private set; }
		public int[] UnityVersion { get; private set; }
		public Il2CppManager(AssetLayout layout, Action<string> requestAssemblyCallback) : base(layout, requestAssemblyCallback) { }

		public override bool IsSet => true;

		public override void Initialize(PlatformGameStructure gameStructure)
		{
			string gameDataPath = gameStructure.GameDataPath;
			if (string.IsNullOrWhiteSpace(gameDataPath)) throw new ArgumentNullException(nameof(gameDataPath));

			GameDataPath = gameStructure.GameDataPath;
			GameAssemblyPath = gameStructure.Il2CppGameAssemblyPath;
			UnityPlayerPath = gameStructure.UnityPlayerPath;
			MetaDataPath = gameStructure.Il2CppMetaDataPath;

			if (gameStructure.UnityVersion != null)
				UnityVersion = gameStructure.UnityVersion;
			else if (gameStructure.UnityPlayerPath != null)
				UnityVersion = Cpp2IlApi.DetermineUnityVersion(UnityPlayerPath, GameDataPath);

			if (UnityVersion == null)
				throw new NullReferenceException("gameStructure.UnityVersion and gameStructure.UnityPlayerPath cannot both be null");
			else
				Logger.Log(LogType.Info, LogCategory.Import, $"During Il2Cpp initialization, found Unity version: {MakeVersionString(UnityVersion)}");

			Cpp2IlApi.InitializeLibCpp2Il(GameAssemblyPath, MetaDataPath, UnityVersion);

			Cpp2IlApi.MakeDummyDLLs(true);

			if(LibCpp2IL.LibCpp2IlMain.Binary is LibCpp2IL.PE.PE)
			{
				var keyFunctionAddresses = Cpp2IlApi.ScanForKeyFunctionAddresses();

				Cpp2IlApi.RunAttributeRestorationForAllAssemblies(keyFunctionAddresses);
			}

			//Cpp2IlApi.SaveAssemblies("ExtractedScripts");

			foreach (var assembly in Cpp2IlApi.GeneratedAssemblies)
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
			if (version == null || version.Length == 0) return "";

			StringBuilder builder = new StringBuilder();
			builder.Append(version[0].ToString());
			for (int i = 1; i < version.Length; i++)
			{
				builder.Append(".");
				builder.Append(version[i].ToString());
			}
			return builder.ToString();
		}

		~Il2CppManager()
		{
			Dispose(false);
		}
	}
}
