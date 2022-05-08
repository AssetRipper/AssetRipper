namespace AssetRipper.Library.Exporters.Scripts
{
	/// <summary>
	/// Settings for code cleanup to make CSharp code compilable
	/// </summary>
	public class CodeCleanupSettings
	{
		/// <summary>
		/// Removes all members that start with <c>&lt;</c>
		/// </summary>
		public bool RemoveInvalidMembers { get; set; } = true;
		/// <summary>
		/// Ensures out parameters are set by setting them to default at the beginning
		/// of methods.
		/// </summary>
		public bool EnsureOutParametersSet { get; set; } = true;
		/// <summary>
		/// Ensures all fields of a struct are set from constructors by setting them
		/// to default at the beginning of the constructor.
		/// </summary>
		public bool EnsureStructFieldsSetInConstructor { get; set; } = true;
		/// <summary>
		/// Ensures all constructors call a valid base constructor.
		/// </summary>
		public bool EnsureValidBaseConstructor { get; set; } = true;
	}
}
