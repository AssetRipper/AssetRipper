namespace AssetRipper.AssemblyDumper.Types;

public static class InterfaceUtils
{
	public const MethodAttributes InterfaceMethodImplementation =
		MethodAttributes.Public |
		MethodAttributes.Final |
		MethodAttributes.HideBySig |
		MethodAttributes.NewSlot |
		MethodAttributes.Virtual;
	public const MethodAttributes InterfaceMethodDeclaration =
		MethodAttributes.Public |
		MethodAttributes.Abstract |
		MethodAttributes.HideBySig |
		MethodAttributes.NewSlot |
		MethodAttributes.Virtual;
	public const MethodAttributes InterfacePropertyImplementation =
		InterfaceMethodImplementation |
		MethodAttributes.SpecialName;
	public const MethodAttributes InterfacePropertyDeclaration =
		InterfaceMethodDeclaration |
		MethodAttributes.SpecialName;

	public static void AddInterfaceImplementation<T>(this TypeDefinition type, CachedReferenceImporter importer)
	{
		type.Interfaces.Add(new InterfaceImplementation(importer.ImportType<T>()));
	}

	public static void AddInterfaceImplementation(this TypeDefinition type, ITypeDefOrRef interfaceReference)
	{
		type.Interfaces.Add(new InterfaceImplementation(interfaceReference));
	}

	public static TypeSignature GetPropertyTypeSignature<T>(string propertyName, CachedReferenceImporter importer)
	{
		return importer.UnderlyingImporter.ImportTypeSignature(
			importer.
			LookupType<T>()!.
			Properties.
			Single(p => p.Name == propertyName).
			Signature!.
			ReturnType);
	}
}
