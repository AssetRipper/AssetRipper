using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass300_NamedInterface
{
	private const string PropertyName = nameof(INamed.Name);

	public static void DoPass()
	{
		TypeSignature utf8StringSignature = SharedState.Instance.Importer.ImportType<Utf8String>().ToTypeSignature();
		ITypeDefOrRef hasNameInterface = SharedState.Instance.Importer.ImportType<INamed>();
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			DoPassOnGroup(group, hasNameInterface, utf8StringSignature);
		}
	}

	private static void DoPassOnGroup(ClassGroupBase group, ITypeDefOrRef hasNameInterface, TypeSignature utf8StringSignature)
	{
		if (group.Types.All(t => t.TryGetNameField(true, out var _)))
		{
			TypeDefinition groupInterface = group.Interface;
			groupInterface.AddInterfaceImplementation(hasNameInterface);
			if (groupInterface.Properties.Any(p => p.Name == PropertyName))
			{
				throw new Exception("Interface already has a name property");
			}

			foreach (TypeDefinition type in group.Types)
			{
				if (type.TryGetNameField(false, out FieldDefinition? field))
				{
					type.ImplementNameProperty(field, utf8StringSignature);
				}
			}
		}
		else
		{
			foreach (TypeDefinition type in group.Types)
			{
				if (type.TryGetNameField(false, out FieldDefinition? field))
				{
					type.AddInterfaceImplementation(hasNameInterface);
					type.ImplementNameProperty(field, utf8StringSignature);
				}
			}
		}
	}

	private static void ImplementNameProperty(this TypeDefinition type, FieldDefinition field, TypeSignature utf8StringSignature)
	{
		if (!type.Properties.Any(p => p.Name == PropertyName))
		{
			type.ImplementFullProperty(PropertyName, InterfaceUtils.InterfacePropertyImplementation, utf8StringSignature, field);
		}
	}

	private static bool TryGetNameField(this TypeDefinition type, bool checkBaseTypes, [NotNullWhen(true)] out FieldDefinition? field)
	{
		field = type.TryGetFieldByName("m_Name", checkBaseTypes);
		return field?.Signature?.FieldType.Name == nameof(Utf8String);
	}
}
