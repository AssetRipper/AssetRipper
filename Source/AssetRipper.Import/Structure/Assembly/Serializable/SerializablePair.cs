using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Traversal;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

public sealed class SerializablePair
{
	public int Depth { get; }
	public SerializableType Type { get; }
	public SerializableValue First { get; }
	public SerializableValue Second { get; }
	private SerializableType.Field FirstField => Type.Fields[0];
	private SerializableType.Field SecondField => Type.Fields[1];

	public SerializablePair(SerializableType type, int depth)
	{
		if (type.Fields.Count != 2)
		{
			throw new ArgumentException("Pair type must have exactly two fields", nameof(type));
		}

		Type = type;
		Depth = depth;
	}

	public void Read(ref EndianSpanReader reader, UnityVersion version, TransferInstructionFlags flags)
	{
		First.Read(ref reader, version, flags, Depth, FirstField);
		Second.Read(ref reader, version, flags, Depth, SecondField);
	}

	public void Write(AssetWriter writer)
	{
		First.Write(writer, FirstField);
		Second.Write(writer, SecondField);
	}

	public void WalkEditor(AssetWalker walker)
	{
		if (Type.Type is PrimitiveType.MapPair)
		{
			// Needs to also handle GUID and Hash128, but those are not used in PlayerSettings, so it doesn't matter right now.
			if (FirstField.Type.Type == PrimitiveType.String)
			{
				KeyValuePair<string, SerializableValue> pair = new(First.AsString, Second);
				if (walker.EnterDictionaryPair(pair))
				{
					walker.VisitPrimitive(pair.Key);
					walker.DividePair(pair);
					Second.WalkEditor(walker, SecondField);
					walker.ExitDictionaryPair(pair);
				}
			}
			else
			{
				KeyValuePair<SerializableValue, SerializableValue> pair = new(First, Second);
				if (walker.EnterDictionaryPair(pair))
				{
					First.WalkEditor(walker, FirstField);
					walker.DivideDictionaryPair(pair);
					Second.WalkEditor(walker, SecondField);
					walker.ExitDictionaryPair(pair);
				}
			}
		}
		else
		{
			KeyValuePair<SerializableValue, SerializableValue> pair = new(First, Second);
			if (walker.EnterPair(pair))
			{
				First.WalkEditor(walker, FirstField);
				walker.DividePair(pair);
				Second.WalkEditor(walker, SecondField);
				walker.ExitPair(pair);
			}
		}
	}

	public void Initialize(UnityVersion version)
	{
		First.Initialize(version, Depth, FirstField);
		Second.Initialize(version, Depth, SecondField);
	}

	public void CopyValues(SerializablePair source, PPtrConverter converter)
	{
		First.CopyValues(source.First, Depth, FirstField, converter);
		Second.CopyValues(source.Second, Depth, SecondField, converter);
	}

	public void Reset()
	{
		First.Reset();
		Second.Reset();
	}
}
