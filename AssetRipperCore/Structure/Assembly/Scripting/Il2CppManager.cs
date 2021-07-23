using AssetRipper.Logging;
using System;
using System.IO;

namespace AssetRipper.Structure.Assembly.Scripting
{
	internal sealed class Il2CppManager : BaseManager
	{
		public const string GameAssemblyName = "GameAssembly.dll";

		public Il2CppManager(AssemblyManager assemblyManager) : base(assemblyManager) { }

		
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
