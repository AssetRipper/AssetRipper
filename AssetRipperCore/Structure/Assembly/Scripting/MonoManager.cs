using System;

namespace AssetRipper.Structure.Assembly.Scripting
{
	internal sealed class MonoManager : BaseManager
	{
		public const string AssemblyExtension = ".dll";

		public MonoManager(AssemblyManager assemblyManager) : base(assemblyManager) { }

		public static bool IsMonoAssembly(string fileName)
		{
			if (fileName.EndsWith(AssemblyExtension, StringComparison.Ordinal))
			{
				return true;
			}
			return false;
		}
	}
}
