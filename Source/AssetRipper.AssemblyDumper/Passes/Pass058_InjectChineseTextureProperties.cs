using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Passes;

/// <summary>
/// A special case for Chinese textures containing an extra 24 bytes at the end.
/// </summary>
internal static class Pass058_InjectChineseTextureProperties
{
	//int m_OriginalWidth // ByteSize{4}, Index{26}, Version{1}, IsArray{0}, MetaFlag{10}
	//int m_OriginalHeight // ByteSize{4}, Index{27}, Version{1}, IsArray{0}, MetaFlag{10}
	//GUID m_OriginalAssetGuid // ByteSize{10}, Index{28}, Version{1}, IsArray{0}, MetaFlag{10}
	//unsigned int data[0] // ByteSize{4}, Index{29}, Version{1}, IsArray{0}, MetaFlag{10}
	//unsigned int data[1] // ByteSize{4}, Index{2a}, Version{1}, IsArray{0}, MetaFlag{10}
	//unsigned int data[2] // ByteSize{4}, Index{2b}, Version{1}, IsArray{0}, MetaFlag{10}
	//unsigned int data[3] // ByteSize{4}, Index{2c}, Version{1}, IsArray{0}, MetaFlag{10}

	private static TypeSignature Int32Type => SharedState.Instance.Importer.Int32;

	public static void DoPass()
	{
		ClassGroup textureGroup = SharedState.Instance.ClassGroups[28];
		ClassGroup cubemapGroup = SharedState.Instance.ClassGroups[89];

		SubclassGroup guidGroup = SharedState.Instance.SubclassGroups["GUID"];
		TypeSignature guidType = guidGroup.Types.Single().ToTypeSignature();

		AddPropertyToBothGroups(textureGroup, cubemapGroup, Int32Type, "m_OriginalWidth");
		AddPropertyToBothGroups(textureGroup, cubemapGroup, Int32Type, "m_OriginalHeight");
		AddPropertyToBothGroups(textureGroup, cubemapGroup, guidType, "m_OriginalAssetGuid");
	}

	private static void AddPropertyToBothGroups(ClassGroup textureGroup, ClassGroup cubemapGroup, TypeSignature fieldType, string fieldName)
	{
		AddProperty(textureGroup, fieldType, fieldName, static (instance, fieldType, fieldName) =>
		{
			FieldDefinition field = instance.Type.AddField(fieldName, fieldType, visibility: Visibility.Internal);
			field.AddDebuggerBrowsableNeverAttribute();
			return field;
		});
		AddProperty(cubemapGroup, fieldType, fieldName, static (instance, fieldType, fieldName) =>
		{
			return instance.Base!.Type.GetFieldByName(fieldName);
		});
	}

	private static void AddProperty(
		ClassGroup group,
		TypeSignature fieldType,
		string fieldName,
		Func<GeneratedClassInstance, TypeSignature, string, FieldDefinition> fieldDelegate)
	{
		string propertyName = GeneratedInterfaceUtils.GetPropertyNameFromFieldName(fieldName, group);

		DataMemberHistory history = new();
		history.Exists.Add(group.MinimumVersion, true);
		history.DocumentationString.Add(group.MinimumVersion, "Injected for chinese textures.");
		history.Name = fieldName;
		if (fieldType is CorLibTypeSignature)
		{
			history.TypeFullName.Add(group.MinimumVersion, new DocExtraction.MetaData.FullNameRecord(fieldType.FullName, fieldType.Name!));
		}

		bool useFullProperty = fieldType.IsValueType;
		InjectedInterfaceProperty interfaceProperty;
		{
			PropertyDefinition property = useFullProperty
				? group.Interface.AddFullProperty(
					propertyName,
					InterfaceUtils.InterfacePropertyDeclaration,
					fieldType)
				: group.Interface.AddGetterProperty(
					propertyName,
					InterfaceUtils.InterfacePropertyDeclaration,
					fieldType);
			interfaceProperty = new InjectedInterfaceProperty(property, group)
			{
				History = history,
			};
			group.InterfaceProperties.Add(interfaceProperty);

		}
		foreach (GeneratedClassInstance instance in group.Instances)
		{
			TypeDefinition type = instance.Type;
			{
				FieldDefinition field = fieldDelegate.Invoke(instance, fieldType, fieldName);
				PropertyDefinition property = useFullProperty
					? type.ImplementFullProperty(
						propertyName,
						InterfaceUtils.InterfacePropertyImplementation,
						fieldType,
						field)
					: type.ImplementGetterProperty(
						propertyName,
						InterfaceUtils.InterfacePropertyImplementation,
						fieldType,
						field);
				InjectedClassProperty classProperty = new InjectedClassProperty(property, field, interfaceProperty, instance)
				{
					History = history,
				};
				instance.Properties.Add(classProperty);
			}
		}
	}
}
