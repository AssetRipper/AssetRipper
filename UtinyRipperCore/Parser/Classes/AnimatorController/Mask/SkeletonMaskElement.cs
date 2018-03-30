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

		public void Read(AssetStream stream)
		{
			if (IsReadIndex(stream.Version))
			{
				Index = stream.ReadUInt32();
			}
			else
			{
				PathHash = stream.ReadUInt32();
			}
			Weight = stream.ReadSingle();
		}

		public uint Index  { get; private set; }
		public uint PathHash { get; private set; }
		public float Weight { get; private set; }
	}
}
