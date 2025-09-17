using AssetRipper.Numerics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static partial class Pass007_ExtractSubclasses
{
	private readonly struct ClassData
	{
		public readonly string Name;
		public readonly UniversalClass Class;
		public readonly UnityVersionRange VersionRange;

		public ClassData(string name, UniversalClass @class, Range<UnityVersion> versionRange)
		{
			Name = name;
			Class = @class;
			VersionRange = versionRange;
		}
	}
}
