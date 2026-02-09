namespace AssetRipper.AssemblyDumper.Documentation;

internal static class SeeXmlTagGenerator
{
	private static string MakeCRef(string interior)
	{
		return $"<see cref=\"{interior}\"/>";
	}

	public static string MakeCRef(Type type)
	{
		return MakeCRef(XmlUtils.GetStringReference(type));
	}

	public static string MakeCRef(TypeDefinition type)
	{
		return MakeCRef(XmlUtils.GetStringReference(type));
	}

	public static string MakeCRef(PropertyDefinition property)
	{
		return MakeCRef(XmlUtils.GetStringReference(property));
	}

	public static string MakeCRefForClassInterface(int classID)
	{
		return MakeCRef(SharedState.Instance.ClassGroups[classID].Interface);
	}

	public static string MakeCRefForClassInterfaceProperty(int classID, string propertyName)
	{
		return MakeCRef(SharedState.Instance.ClassGroups[classID].Interface.Properties.First(p => p.Name == propertyName));
	}

	public static string MakeCRefForSubclassInterface(string name)
	{
		return MakeCRef(SharedState.Instance.SubclassGroups[name].Interface);
	}

	public static string MakeHRef(string link)
	{
		return $"<see href=\"{link}\"/>";
	}

	public static string MakeHRef(string link, string displayText)
	{
		return $"<see href=\"{link}\">{displayText}</see>";
	}
}
