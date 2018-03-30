namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct HumanPoseMask : IAssetReadable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadSecondWord(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetStream stream)
		{
			m_word0 = stream.ReadUInt32();
			m_word1 = stream.ReadUInt32();
			if (IsReadSecondWord(stream.Version))
			{
				m_word2 = stream.ReadUInt32();
			}
		}
		
		public uint m_word0;
		public uint m_word1;
		public uint m_word2;
	}
}
