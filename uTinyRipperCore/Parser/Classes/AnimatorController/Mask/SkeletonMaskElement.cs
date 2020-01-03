namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct SkeletonMaskElement : IAssetReadable
	{
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasIndex(Version version) => version.IsGreaterEqual(4, 3);

		public void Read(AssetReader reader)
		{
			if (HasIndex(reader.Version))
			{
				Index = reader.ReadUInt32();
			}
			else
			{
				PathHash = reader.ReadUInt32();
			}
			Weight = reader.ReadSingle();
		}

		public uint Index  { get; set; }
		public uint PathHash { get; set; }
		public float Weight { get; set; }
	}
}
