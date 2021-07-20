using AssetRipper.Parser.IO.Asset.Reader;

namespace AssetRipper.Parser.Classes.AnimationClip
{
	public struct ValueDelta : IAssetReadable
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
