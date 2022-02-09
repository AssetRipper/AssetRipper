namespace AssetRipper.Core.Interfaces
{
	public interface IDeepCloneable
	{
		object DeepClone();
	}

	public static class DeepCloneableExtensions
	{
		public static T DeepClone<T>(IDeepCloneable cloneable) => (T)cloneable.DeepClone();
	}
}
