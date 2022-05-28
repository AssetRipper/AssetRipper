namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class UnityVersionEndAttribute : Attribute
	{
		public UnityVersionEndAttribute(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
		{
			Major = major;
			Minor = minor;
			Build = build;
			Type = type;
			TypeNumber = typeNumber;
		}

		public ushort Major { get; }
		public ushort Minor { get; }
		public ushort Build { get; }
		public UnityVersionType Type { get; }
		public byte TypeNumber { get; }

		public UnityVersion ToUnityVersion() => new UnityVersion(Major, Minor, Build, Type, TypeNumber);
	}
}
