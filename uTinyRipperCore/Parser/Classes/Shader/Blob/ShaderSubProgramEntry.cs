namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderSubProgramEntry : IAssetReadable, IAssetWritable
	{
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasSegment(Version version) => version.IsGreaterEqual(2019, 3);

		public void Read(AssetReader reader)
		{
			Offset = reader.ReadInt32();
			Length = reader.ReadInt32();
			if (HasSegment(reader.Version))
			{
				Segment = reader.ReadInt32();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Offset);
			writer.Write(Length);
			if (HasSegment(writer.Version))
			{
				writer.Write(Segment);
			}
		}

		public int Offset { get; set; }
		public int Length { get; set; }
		public int Segment { get; set; }
	}
}
