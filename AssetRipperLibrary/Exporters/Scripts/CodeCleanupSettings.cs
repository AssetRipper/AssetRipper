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
		/// <summary>
		/// Ensures all optional parameters have a default value set to them.
		/// </summary>
		public bool ValidateOptionalParameterValues { get; set; } = true;
		/// <summary>
		/// Replaces all null casts with default casts
		/// </summary>
		public bool ValidateNullCasts { get; set; } = true;
		/// <summary>
		/// Attempts to fix all explicit interface implementations (e.g. special name methods)
		/// </summary>
		public bool FixExplicitInterfaceImplementations { get; set; } = true;
	}
}
