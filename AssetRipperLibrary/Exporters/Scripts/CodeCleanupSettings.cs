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
	}
}
