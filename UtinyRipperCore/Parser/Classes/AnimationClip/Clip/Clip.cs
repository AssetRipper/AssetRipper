namespace UtinyRipper.Classes.AnimationClips
{
	public struct Clip : IAssetReadable
	{
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadConstantClip(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}

		public void Read(AssetStream stream)
		{
			StreamedClip.Read(stream);
			DenseClip.Read(stream);
			if (IsReadConstantClip(stream.Version))
			{
				ConstantClip.Read(stream);
			}
			Binding.Read(stream);
		}

		public bool IsValid(Version version)
		{
			if (StreamedClip.IsValid)
			{
				return true;
			}
			if (DenseClip.IsValid)
			{
				return true;
			}
			if (IsReadConstantClip(version))
			{
				if (ConstantClip.IsValid)
				{
					return true;
				}
			}
			return false;
		}

		public StreamedClip StreamedClip;
		public DenseClip DenseClip;
		public ConstantClip ConstantClip;
		public ValueArrayConstant Binding;
	}
}
