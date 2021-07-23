using AssetRipper.Logging;
using System;
using System.IO;
using System.Text;
using Cpp2IlApi = Cpp2IL.Core.Cpp2IlApi;

namespace AssetRipper.Structure.Assembly.Scripting
{
	internal sealed class Il2CppManager : BaseManager
	{
		public const string GameAssemblyName = "GameAssembly.dll";
		public const string UnityPlayerName = "UnityPlayer.dll";

		public string GameAssemblyPath { get; private set; }
		public string UnityPlayerPath { get; private set; }
		public string GameDataPath { get; private set; }
		public string RootPath { get; private set; }
		public string MetaDataPath { get; private set; }
		public int[] UnityVersion { get; private set; }
		public Il2CppManager(AssemblyManager assemblyManager) : base(assemblyManager) { }

		public override void Initialize(string gameDataPath)
		{
			if (string.IsNullOrWhiteSpace(gameDataPath)) throw new ArgumentNullException(nameof(gameDataPath));

			GameDataPath = Path.GetFullPath(gameDataPath);
			RootPath = (new DirectoryInfo(GameDataPath)).Parent.FullName;
			GameAssemblyPath = Path.Combine(RootPath, GameAssemblyName);
			UnityPlayerPath = Path.Combine(RootPath, UnityPlayerName);
			MetaDataPath = Path.Combine(GameDataPath, "il2cpp_data", "Metadata", "global-metadata.dat");
			UnityVersion = Cpp2IlApi.DetermineUnityVersion(UnityPlayerPath, GameDataPath);
			Logger.Log(LogType.Info, LogCategory.Import, $"During Il2Cpp initialization, found Unity version: {MakeVersionString(UnityVersion)}");

			Cpp2IlApi.InitializeLibCpp2Il(GameAssemblyPath, MetaDataPath, UnityVersion);

			Cpp2IlApi.MakeDummyDLLs(true);

			var keyFunctionAddresses = Cpp2IlApi.ScanForKeyFunctionAddresses();

			Cpp2IlApi.RunAttributeRestorationForAllAssemblies(keyFunctionAddresses);

			//Cpp2IlApi.SaveAssemblies("ExtractedScripts");

			foreach (var assembly in Cpp2IlApi.GeneratedAssemblies)
			{
				m_assemblies.Add(assembly.Name.Name, assembly);
			}
		}

		public override void Load(string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			if(fileName != GameAssemblyName)
			{
				throw new NotSupportedException("Only Il2Cpp game assemblies can be read.");
			}
			else
			{
				Logger.Log(LogType.Info, LogCategory.Import, $"Trying to load '{fileName}' from {filePath}");
			}
		}

		public override void Read(Stream stream, string fileName)
		{
			throw new NotSupportedException();
		}

		public static bool IsIl2Cpp(string[] assemblyNames)
		{
			if (assemblyNames == null) throw new ArgumentNullException(nameof(assemblyNames));
			foreach (string name in assemblyNames)
			{
				if (name == GameAssemblyName)
					return true;
			}
			return false;
		}

		public static bool IsIl2Cpp(string assemblyName)
		{
			if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));
			else return assemblyName == GameAssemblyName;
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
