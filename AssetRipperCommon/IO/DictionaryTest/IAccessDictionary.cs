using System.Collections.Generic;

namespace AssetRipper.Core.IO.DictionaryTest
{
	/// <summary>
	/// Access the contents of another dictionary
	/// </summary>
	/// <typeparam name="TKey">The exposed key type, such as an interface or primitive type</typeparam>
	/// <typeparam name="TValue">The exposed value type, such as an interface or primitive type</typeparam>
	public interface IAccessDictionary<TKey, TValue>
	{
		/// <summary>
		/// The capacity of the reference dictionary 
		/// </summary>
		int Capacity { get; }

		/// <summary>
		/// The number of pairs in the reference dictionary
		/// </summary>
		int Count { get; }

		/// <summary>
		/// The keys in the reference dictionary
		/// </summary>
		IEnumerable<TKey> Keys { get; }

		/// <summary>
		/// The values in the reference dictionary
		/// </summary>
		IEnumerable<TValue> Values { get; }

		/// <summary>
		/// Add a pair to the reference dictionary
		/// </summary>
		/// <remarks>
		/// This method is not necessarily type safe. 
		/// It could throw exceptions if used improperly.
		/// </remarks>
		/// <param name="key">The key to be added</param>
		/// <param name="value">The value to be added</param>
		void Add(TKey key, TValue value);

		/// <summary>
		/// Add a new pair to the reference dictionary
		/// </summary>
		void AddNew();

		/// <summary>
		/// Get a key in the reference dictionary
		/// </summary>
		/// <param name="index">The index to access</param>
		/// <returns>The key at the specified index</returns>
		TKey GetKey(int index);

		/// <summary>
		/// Get a value in the reference dictionary
		/// </summary>
		/// <param name="index">The index to access</param>
		/// <returns>The value at the specified index</returns>
		TValue GetValue(int index);

		/// <summary>
		/// Set a key in the reference dictionary
		/// </summary>
		/// <remarks>
		/// This method is not necessarily type safe. 
		/// It could throw exceptions if used improperly.
		/// </remarks>
		/// <param name="index">The index to access</param>
		/// <param name="newKey">The new key to be assigned</param>
		void SetKey(int index, TKey newKey);

		/// <summary>
		/// Set a value in the reference dictionary
		/// </summary>
		/// <remarks>
		/// This method is not necessarily type safe. 
		/// It could throw exceptions if used improperly.
		/// </remarks>
		/// <param name="index">The index to access</param>
		/// <param name="newValue">The new value to be assigned</param>
		void SetValue(int index, TValue newValue);

		/// <summary>
		/// Access a pair in the reference dictionary
		/// </summary>
		/// <remarks>
		/// The get method is type safe.
		/// The set method is not necessarily type safe
		/// and could throw exceptions if used improperly.
		/// </remarks>
		/// <param name="index">The index to access</param>
		/// <returns>The pair at that index</returns>
		KeyValuePair<TKey, TValue> this[int index] { get; set; }
	}
}
