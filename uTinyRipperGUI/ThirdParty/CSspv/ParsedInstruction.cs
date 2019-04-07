using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SpirV
{
	public class ParsedOperand
	{
		public IList<uint> Words { get; }
		public object Value { get; set; }

		public Operand Operand { get; }

		public ParsedOperand (IList<uint> words, object value, Operand operand)
		{
			Words = words;
			Value = value;
			Operand = operand;
		}

		public T GetSingleEnumValue<T> () where T : System.Enum
		{
			var v = Value as IValueEnumOperandValue;

			if (v.Value.Count == 0) {
				// If there's no value at all, the enum is probably something
				// like ImageFormat. In which case we just return the enum value
				return (T)(object)v.Key;
			} else {
				// This means the enum has a value attached to it, so we return
				// the attached value
				return (T)((IValueEnumOperandValue)Value).Value.First();
			}
		}

		public uint GetId ()
		{
			return (Value as ObjectReference).Id;
		}

		public T GetBitEnumValue<T> () where T : System.Enum
		{
			var v = Value as IBitEnumOperandValue;

			uint result = 0;
			foreach (var k in v.Values.Keys)
			{
				result |= k;
			}

			return (T)(object)result;
		}
	}

	public class VaryingOperandValue
	{
		public VaryingOperandValue (List<object> values)
		{
			Values = values;
		}

		public override string ToString()
		{
			var sb = new StringBuilder ();

			for (int i = 0; i < Values.Count; ++i) {
				sb.Append (Values[i]);

				if (i < (Values.Count - 1)) {
					sb.Append (" ");
				}
			}

			return sb.ToString ();
		}

		public IReadOnlyList<object> Values {get;}
	}

	public interface IEnumOperandValue
	{
		System.Type EnumerationType { get; }
	}

	public interface IBitEnumOperandValue : IEnumOperandValue
	{
		IReadOnlyDictionary<uint, List<object>> Values { get; }
	}

	public interface IValueEnumOperandValue : IEnumOperandValue
	{
		object Key { get; }
		List<object> Value { get; }
	}

	public class ValueEnumOperandValue<T> : IValueEnumOperandValue where T : System.Enum
	{
		public System.Type EnumerationType { get { return typeof(T); } }

		public object Key { get { return key_; } }
		public List<object> Value { get; }

		private readonly T key_ = default;

		public ValueEnumOperandValue (T key, List<object> value)
		{
			key_ = key;
			Value = value;
		}
	}

	public class BitEnumOperandValue<T> : IBitEnumOperandValue where T : System.Enum
	{
		public IReadOnlyDictionary<uint, List<object>> Values { get; }
		public System.Type EnumerationType { get { return typeof(T); } }

		public BitEnumOperandValue (Dictionary<uint, List<object>> values)
		{
			Values = values;
		}
	}

	public class ObjectReference
	{
		public ObjectReference (uint id)
		{
			Id = id;
		}

		public void Resolve (IReadOnlyDictionary<uint, ParsedInstruction> objects)
		{
			object_ = objects [Id];
		}

		public uint Id { get; }
		public ParsedInstruction Reference { get => object_; }

		private ParsedInstruction object_;

		public override string ToString ()
		{
			return $"%{Id}";
		}
	}

	public class ParsedInstruction
	{
		public IList<uint> Words { get; }

		public Instruction Instruction { get; }
		public IList<ParsedOperand> Operands { get; } = new List<ParsedOperand> ();

		public string Name { get; set; }

		public object Value { get; set; }

		public ParsedInstruction (int opCode, IList<uint> words)
		{
			Words = words;

			Instruction = Instructions.OpcodeToInstruction [opCode];

			ParseOperands ();
		}

		private void ParseOperands ()
		{
			if (Instruction.Operands.Count == 0) {
				return;
			}

			// Word 0 describes this instruction so we can ignore it
			int currentWord = 1;
			int currentOperand = 0;

			var varyingOperandValues = new List<object> ();
			var varyingWordStart = 0;
			Operand varyingOperand = null;

			for (; currentWord < Words.Count;) {
				var operand = Instruction.Operands [currentOperand];

				operand.Type.ReadValue (Words.Skip (currentWord).ToList (),
					out object value, out int wordsUsed);

				if (operand.Quantifier == OperandQuantifier.Varying) {
					varyingOperandValues.Add (value);
					varyingWordStart = currentWord;
					varyingOperand = operand;
				} else {
					Operands.Add (new ParsedOperand (Words.Skip (currentWord).Take (wordsUsed).ToList (),
						value, operand));
				}

				currentWord += wordsUsed;

				if (operand.Quantifier != OperandQuantifier.Varying) {
					++currentOperand;
				}
			}

			if (varyingOperand != null) {
				Operands.Add (new ParsedOperand (Words.Skip (currentWord).ToList (),
						new VaryingOperandValue (varyingOperandValues), varyingOperand));
			}
		}

		public Type ResultType { get; set; }
		public uint ResultId { get
			{
				for (int i = 0; i < Instruction.Operands.Count; ++i) {
					if (Instruction.Operands[i].Type is IdResult) {
						return Operands[i].GetId ();
					}
				}

				return 0;
			}
		}

		public bool HasResult { get => ResultId != 0; }

		public void ResolveResultType (IReadOnlyDictionary<uint, ParsedInstruction> objects)
		{
			if (Instruction.Operands.Count > 0 && Instruction.Operands [0].Type is IdResultType) {
				ResultType = objects[(uint)Operands[0].Value].ResultType;
			}
		}

		public void ResolveReferences (IReadOnlyDictionary<uint, ParsedInstruction> objects)
		{
			foreach (var operand in Operands) {
				if (operand.Value is ObjectReference objectReference) {
					objectReference.Resolve (objects);
				}
			}
		}
	}
}
