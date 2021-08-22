namespace ShaderTextRestorer
{
	internal class Sampler
	{
		public string Name;
		public uint BindPoint;
		public bool IsComparisonSampler;
		public Sampler(string name, uint bindPoint, bool isComparisonSampler)
		{
			this.Name = name;
			this.BindPoint = bindPoint;
			this.IsComparisonSampler = isComparisonSampler;
		}
	}
}
