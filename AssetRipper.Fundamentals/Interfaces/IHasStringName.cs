namespace AssetRipper.Core.Interfaces
{
	public interface IHasNameString
	{
		string NameString { get; set; }
	}

	public static class HasNameExtensions
	{
		/// <summary>
		/// Get a non-empty name for the object
		/// </summary>
		/// <param name="named">The object implementing the HasName interface</param>
		/// <returns>The object's name if it's not empty, otherwise the name of the object's type</returns>
		public static string GetNameNotEmpty(this IHasNameString named)
		{
			return string.IsNullOrEmpty(named.NameString) ? named.GetType().Name : named.NameString;
		}
	}
}
