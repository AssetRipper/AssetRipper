namespace AssetRipper.Library.Configuration
{
	public enum ScriptExportMode
	{
		/// <summary>
		/// Uses the ILSpy decompiler to generate CS scripts. This is reliable. However, it's also time-consuming and contains many compile errors.
		/// </summary>
		Decompiled,
		/// <summary>
		/// Special assemblies, such as Assembly-CSharp, are decompiled to CS scripts with the ILSpy decompiler. Other assemblies are saved as DLL files.
		/// </summary>
		Hybrid,
		/// <summary>
		/// Exports assemblies in their compiled Dll form. Highly experimental. Might not work at all.
		/// </summary>
		TotalDllExport,
	}
}
