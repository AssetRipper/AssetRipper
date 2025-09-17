using AssetRipper.AssemblyDumper.Types;
using System.Runtime.CompilerServices;

namespace AssetRipper.AssemblyDumper.Fields;

/// <summary>
/// Helper class for creating fixed size buffer fields<br/>
/// <seealso href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code#fixed-size-buffers"/>
/// </summary>
/// <remarks>
/// The array type must be bool, byte, char, short, int, long, sbyte, ushort, uint, ulong, float, or double.
/// </remarks>
public static class FixedSizeBufferCreator
{
	public static FieldDefinition AddFixedSizeBufferField(this TypeDefinition declaringType, AssemblyBuilder builder, CorLibTypeSignature arrayElementType, string fieldName, int length)
	{
		uint size = (uint)(length * GetSize(arrayElementType));
		TypeDefinition nestedType = MakeNestedType(declaringType, builder, arrayElementType, fieldName, size);
		FieldDefinition field = declaringType.AddField(fieldName, nestedType.ToTypeSignature());
		IMethodDefOrRef attributeConstructor = builder.Importer.ImportMethod<FixedBufferAttribute>(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 2);
		field.AddCustomAttribute(attributeConstructor,
			(builder.Importer.ImportTypeSignature<Type>(), arrayElementType),
			(builder.Importer.Int32, length));
		return field;
	}

	private static string GetNestedTypeName(string fieldName) => $"<{fieldName}>e__FixedBuffer";

	private static TypeDefinition MakeNestedType(TypeDefinition parentType, AssemblyBuilder builder, CorLibTypeSignature arrayElementType, string fieldName, uint size)
	{
		TypeDefinition nestedType = StructCreator.CreateEmptyStruct(builder, parentType, GetNestedTypeName(fieldName));
		nestedType.ClassLayout = new ClassLayout(0, size);
		nestedType.AddField("FixedElementField", arrayElementType);
		nestedType.AddCustomAttribute(builder.Importer.ImportDefaultConstructor<CompilerGeneratedAttribute>());
		nestedType.AddCustomAttribute(builder.Importer.ImportDefaultConstructor<UnsafeValueTypeAttribute>());
		return nestedType;
	}

	private static IMethodDefOrRef ImportDefaultConstructor<T>(this CachedReferenceImporter importer)
	{
		return importer.ImportMethod<T>(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0);
	}

	private static int GetSize(CorLibTypeSignature type)
	{
		return type.ElementType switch
		{
			ElementType.U1 => 1,
			ElementType.U2 => 2,
			ElementType.U4 => 4,
			ElementType.U8 => 8,
			ElementType.I1 => 1,
			ElementType.I2 => 2,
			ElementType.I4 => 4,
			ElementType.I8 => 8,
			ElementType.R4 => 4,
			ElementType.R8 => 8,
			ElementType.Boolean => 1,
			ElementType.Char => 2,
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}
}
