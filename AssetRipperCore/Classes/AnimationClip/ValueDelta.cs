using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class ValueDelta : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Start = reader.ReadSingle();
			Stop = reader.ReadSingle();
		}

		public override string ToString()
		{
			return $"Start:{Start} Stop:{Stop}";
		}

		public float Start { get; set; }
		public float Stop { get; set; }
	}
}
