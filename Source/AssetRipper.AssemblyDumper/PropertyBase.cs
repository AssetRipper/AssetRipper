using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.DocExtraction.DataStructures;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal abstract class PropertyBase
{
	protected PropertyBase(PropertyDefinition definition)
	{
		Definition = definition;
	}

	public PropertyDefinition Definition { get; }
	public PropertyDefinition? SpecialDefinition { get; set; }
	public DataMemberHistory? History { get; set; }
	public MethodDefinition? HasMethod { get; set; }
	public MethodDefinition? ReleaseOnlyMethod { get; set; }
	public MethodDefinition? EditorOnlyMethod { get; set; }

	[MemberNotNullWhen(true, nameof(SpecialDefinition))]
	public bool HasEnumVariant
	{
		get
		{
			return SpecialDefinition?.Signature?.ReturnType.ToTypeDefOrRef() is TypeDefinition typeDefinition
				&& typeDefinition.IsEnum;
		}
	}

	public bool HasSetAccessor => Definition.SetMethod != null;

	/// <summary>
	/// The <see cref="PropertyDefinition.Name"/> for <see cref="Definition"/>.
	/// </summary>
	public string Name => Definition.Name ?? "";

	public virtual bool IsInjected => false;

	/// <summary>
	/// Import references for doing an equality comparison of this property.
	/// </summary>
	/// <param name="getReference">A reference to a static, parameterless method returning a <see cref="EqualityComparer{T}"/>.</param>
	/// <param name="equalsReference">A reference to the corresponding <see cref="EqualityComparer{T}.Equals(T?, T?)"/> method.</param>
	public virtual void GetEqualityComparer(out IMethodDefOrRef getReference, out IMethodDefOrRef equalsReference)
	{
		EqualityMethods.MakeEqualityComparerGenericMethods(
			Definition.Signature!.ReturnType,
			SharedState.Instance.Importer,
			out getReference,
			out equalsReference);
	}

	private string GetDebuggerDisplay() => Name;
}
