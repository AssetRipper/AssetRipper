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
			string result = named.NameString;
			if (string.IsNullOrEmpty(result))
			{
				result = named is IUnityObjectBase @object ? @object.AssetClassName : named.GetType().Name;
			}
			return result;
		}
	}
}
