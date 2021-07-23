using AssetRipper.Logging;
using System;
using System.IO;

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

		public void Initialize(string gameAssemblyPath, string gameDataPath)
		{
			if (string.IsNullOrWhiteSpace(gameAssemblyPath)) throw new ArgumentNullException(nameof(gameAssemblyPath));
			if (string.IsNullOrWhiteSpace(gameDataPath)) throw new ArgumentNullException(nameof(gameDataPath));

			GameAssemblyPath = Path.GetFullPath(gameAssemblyPath);
			RootPath = Path.GetDirectoryName(GameAssemblyPath);
			UnityPlayerPath = Path.Combine(RootPath, UnityPlayerName);
			GameDataPath = Path.GetFullPath(gameDataPath);
			MetaDataPath = Path.Combine(GameDataPath, "il2cpp_data", "Metadata", "global-metadata.dat");
			UnityVersion = Cpp2IL.Core.Cpp2IlApi.DetermineUnityVersion(UnityPlayerPath, GameDataPath);

			Cpp2IL.Core.Cpp2IlApi.InitializeLibCpp2Il(GameAssemblyPath, MetaDataPath, UnityVersion);

			//var assemblies = Cpp2IL.Core.Cpp2IlApi.MakeDummyDLLs(true);
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
	}
}
