namespace AssetRipper.Export.Configuration;

public enum ScriptExportMode
{
	/// <summary>
	/// Use the ILSpy decompiler to generate CS scripts. This is reliable. However, it's also time-consuming and contains many compile errors.
	/// </summary>
	Decompiled,
	/// <summary>
	/// Special assemblies, such as Assembly-CSharp, are decompiled to CS scripts with the ILSpy decompiler. Other assemblies are saved as DLL files.
	/// </summary>
	Hybrid,
	/// <summary>
	/// Special assemblies, such as Assembly-CSharp, are renamed to have compatible names.
	/// </summary>
	DllExportWithRenaming,
	/// <summary>
	/// Export assemblies in their compiled Dll form. Experimental. Might not work at all.
	/// </summary>
	DllExportWithoutRenaming,
}
