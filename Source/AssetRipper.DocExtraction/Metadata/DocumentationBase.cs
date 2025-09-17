namespace AssetRipper.DocExtraction.MetaData;

public abstract record class DocumentationBase
{
	public string Name { get; set; } = "";
	/// <summary>
	/// NativeNameAttribute for members, NativeClassAttribute for structs and classes, sometimes NativePropertyAttribute for properties
	/// </summary>
	public string? NativeName { get; set; }
	public string? ObsoleteMessage { get; set; }
	public string? DocumentationString { get; set; }
	public override string ToString()
	{
		return Name;
	}
}
