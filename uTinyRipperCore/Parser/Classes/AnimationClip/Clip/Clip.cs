using uTinyRipper.Classes.AnimatorControllers;

namespace uTinyRipper.Classes.AnimationClips
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
		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public static bool IsReadBinding(Version version)
		{
			return version.IsLess(2018, 3);
		}

		public void Read(AssetReader reader)
		{
			StreamedClip.Read(reader);
			DenseClip.Read(reader);
			if (IsReadConstantClip(reader.Version))
			{
				ConstantClip.Read(reader);
			}
			if (IsReadBinding(reader.Version))
			{
				Binding.Read(reader);
			}
		}

		public bool IsSet(Version version)
		{
			if (StreamedClip.IsSet)
			{
				return true;
			}
			if (DenseClip.IsSet)
			{
				return true;
			}
			if (IsReadConstantClip(version))
			{
				if (ConstantClip.IsSet)
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
