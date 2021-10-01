namespace AssetRipper.Library.Configuration
{
	public enum ScriptExportMode
	{
		/// <summary>
		/// Uses the ILSpy decompiler to generate CS scripts. This is reliable. However, it's also time-consuming and contains many compile errors.
		/// </summary>
		Decompiled,
		/// <summary>
		/// Exports assemblies in their compiled Dll form. Highly experimental. Might not work at all.
		/// </summary>
		Package,
	}
}
