namespace AssetRipper.Core.Imported
{
	public class ImportedKeyframe<T>
	{
		public float time { get; set; }
		public T value { get; set; }

		public ImportedKeyframe(float time, T value)
		{
			this.time = time;
			this.value = value;
		}
	}
}
