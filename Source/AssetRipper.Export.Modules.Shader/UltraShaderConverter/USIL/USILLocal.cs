namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;

public class USILLocal
{
	public string type;
	public string name;
	public USILLocalType usilType;
	public bool isArray;
	public List<USILOperand> defaultValues;

	public USILLocal(string type, string name, USILLocalType usilType, bool isArray = false)
	{
		this.type = type;
		this.name = name;
		this.usilType = usilType;
		this.isArray = isArray;
		defaultValues = new List<USILOperand>();
	}
}
