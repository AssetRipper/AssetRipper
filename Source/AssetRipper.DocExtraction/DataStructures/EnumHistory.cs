using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.DocExtraction.Extensions;
using AssetRipper.DocExtraction.MetaData;
using AssetRipper.Primitives;

namespace AssetRipper.DocExtraction.DataStructures;
public sealed class EnumHistory : TypeHistory<EnumMemberHistory, EnumMemberDocumentation>
{
	public VersionedList<ElementType> ElementType { get; set; } = new();
	public bool IsFlagsEnum { get; set; }

	public static EnumHistory From(UnityVersion version, EnumDocumentation @enum)
	{
		EnumHistory? history = new();
		history.Initialize(version, @enum);
		return history;
	}

	public override void Initialize(UnityVersion version, DocumentationBase first)
	{
		base.Initialize(version, first);
		EnumDocumentation @enum = (EnumDocumentation)first;
		ElementType.Add(version, @enum.ElementType);
		IsFlagsEnum = @enum.IsFlagsEnum;
	}

	protected override void AddNotNull(UnityVersion version, DocumentationBase next)
	{
		base.AddNotNull(version, next);
		EnumDocumentation @enum = (EnumDocumentation)next;
		AddIfNotEqual(ElementType, version, @enum.ElementType);
		IsFlagsEnum |= @enum.IsFlagsEnum;
	}

	public bool TryGetSingleElementType(out ElementType elementType)
	{
		if (ElementType.Count == 1)
		{
			elementType = ElementType[0].Value;
			return true;
		}
		else
		{
			elementType = default;
			return false;
		}
	}

	public bool TryGetMergedElementType(out ElementType elementType)
	{
		if (ElementType.Count == 0)
		{
			elementType = default;
			return false;
		}
		else if (ElementType.Count == 1)
		{
			elementType = ElementType[0].Value;
			return true;
		}
		else
		{
			try
			{
				elementType = ElementType[0].Value;
				for (int i = 1; i < ElementType.Count; i++)
				{
					elementType = elementType.Merge(ElementType[i].Value);
				}
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
				elementType = default;
				return false;
			}
		}
	}

}
