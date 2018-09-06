namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct SkeletonMaskElement : IAssetReadable
	{
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadIndex(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}

		public void Read(AssetReader reader)
		{
			if (IsReadIndex(reader.Version))
			{
				Index = reader.ReadUInt32();
			}
			else
			{
				PathHash = reader.ReadUInt32();
			}
			Weight = reader.ReadSingle();
		}

		public uint Index  { get; private set; }
		public uint PathHash { get; private set; }
		public float Weight { get; private set; }
	}
}
