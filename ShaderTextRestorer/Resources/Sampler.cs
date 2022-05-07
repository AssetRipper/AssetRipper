namespace ShaderTextRestorer.Resources
{
	internal class Sampler
	{
		public string Name;
		public uint BindPoint;
		public bool IsComparisonSampler;
		public Sampler(string name, uint bindPoint, bool isComparisonSampler)
		{
			Name = name;
			BindPoint = bindPoint;
			IsComparisonSampler = isComparisonSampler;
		}
	}
}
