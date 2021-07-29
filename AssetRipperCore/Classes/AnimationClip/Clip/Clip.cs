using AssetRipper.Classes.AnimatorController.Constants;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;

namespace AssetRipper.Classes.AnimationClip.Clip
{
	public class Clip : IAssetReadable
	{
		public Clip() { }

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasConstantClip(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public static bool HasBinding(Version version) => version.IsLess(2018, 3);

		public void Read(AssetReader reader)
		{
			StreamedClip.Read(reader);
			DenseClip.Read(reader);
			if (HasConstantClip(reader.Version))
			{
				ConstantClip.Read(reader);
			}
			if (HasBinding(reader.Version))
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
			if (HasConstantClip(version))
			{
				if (ConstantClip.IsSet)
				{
					return true;
				}
			}
			return false;
		}

		public StreamedClip StreamedClip = new StreamedClip();
		public DenseClip DenseClip = new DenseClip();
		public ConstantClip ConstantClip = new ConstantClip();
		public ValueArrayConstant Binding;
	}
}
