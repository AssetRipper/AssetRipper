namespace AssetRipper.Core.Interfaces
{
	public interface IHasName
	{
		string Name { get; set; }
	}

	public static class HasNameExtensions
	{
		/// <summary>
		/// Get a non-empty name for the object
		/// </summary>
		/// <param name="named">The object implementing the HasName interface</param>
		/// <returns>The object's name if it's not empty, otherwise the name of the object's type</returns>
		public static string GetNameNotEmpty(this IHasName named)
		{
			return string.IsNullOrEmpty(named.Name) ? named.GetType().Name : named.Name;
		}
	}
}
