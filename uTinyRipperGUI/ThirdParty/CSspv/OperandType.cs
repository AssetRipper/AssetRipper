using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace SpirV
{
	public class OperandType
	{
		public virtual bool ReadValue (IList<uint> words, 
			out object value, out int wordsUsed)
		{
			// This returns the dynamic type
			value = GetType ();
			wordsUsed = 1;

			return true;
		}
	}

	public class Literal : OperandType
	{

	}

	public class LiteralNumber : Literal
	{
	}

	// The SPIR-V JSON file uses only literal integers
	public class LiteralInteger : LiteralNumber
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = words [0];
			wordsUsed = 1;

			return true;
		}
	}

	public class LiteralString : Literal
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			StringBuilder sb = new StringBuilder ();
			// This is just a fail-safe -- the loop below must terminate
			wordsUsed = 1;

			List<byte> bytes = new List<byte> ();
			for (int i = 0; i < words.Count; ++i) {
				var wordBytes = BitConverter.GetBytes (words [i]);

				int zeroOffset = -1;
				for (int j = 0; j < wordBytes.Length; ++j) {
					if (wordBytes [j] == 0) {
						zeroOffset = j;
						break;
					} else {
						bytes.Add (wordBytes [j]);
					}
				}

				if (zeroOffset != -1) {
					wordsUsed = i + 1;
					break;
				}
			}

			var decoder = new UTF8Encoding ();
			var byteArray = bytes.ToArray ();
			value = decoder.GetString (byteArray, 0, byteArray.Length);

			return true;
		}
	}

	public class LiteralContextDependentNumber : Literal
	{
		// This is handled during parsing by ConvertConstant
	}

	public class LiteralExtInstInteger : Literal
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = words[0];
			wordsUsed = 1;

			return true;
		}
	}

	public class LiteralSpecConstantOpInteger : Literal
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			List<ObjectReference> result = new List<ObjectReference> ();
			foreach (var w in words) {
				result.Add (new ObjectReference (w));
			}

			value = result;
			wordsUsed = words.Count;

			return true;
		}
	}

	public class Parameter
	{
		public virtual IReadOnlyList<OperandType> OperandTypes { get; }
	}

	public class ParameterFactory
	{
		public virtual Parameter CreateParameter(object value) => null;
	}

	public class EnumType<T> : EnumType<T, ParameterFactory> where T : System.Enum { };

	public class EnumType<T, U> : OperandType where T : System.Enum where U : ParameterFactory, new ()
	{
		private U parameterFactory_ = new U();
		public System.Type EnumerationType { get { return typeof(T); } }

		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			var wordsUsedForParameters = 0;

			if (typeof(T).GetTypeInfo().GetCustomAttributes<FlagsAttribute> ().Any ()) {
				var result = new Dictionary<uint, List<object>> ();
				foreach (var enumValue in EnumerationType.GetEnumValues()) {
					var bit = (uint)enumValue;

					// bit == 0 and words [0] == 0 handles the 0x0 = None cases
					if ((words [0] & bit) != 0 || (bit == 0 && words[0] == 0)) {
						var p = parameterFactory_.CreateParameter (bit);

						var resultItems = new List<object> ();

						if (p != null) {
							for (int j = 0; j < p.OperandTypes.Count; ++j) {
								p.OperandTypes [j].ReadValue (
									words.Skip (1 + wordsUsedForParameters).ToList (),
									out object pValue, out int pWordsUsed);
								wordsUsedForParameters += pWordsUsed;
								resultItems.Add (pValue);
							}
						}

						result [bit] = resultItems;
					}
				}

				value = new BitEnumOperandValue<T> (result);
			} else {
				var resultItems = new List<object> ();
				var p = parameterFactory_.CreateParameter(words [0]);
				if (p != null) {
					for (int j = 0; j < p.OperandTypes.Count; ++j) {
						p.OperandTypes [j].ReadValue (
							words.Skip (1 + wordsUsedForParameters).ToList (),
							out object pValue, out int pWordsUsed);
						wordsUsedForParameters += pWordsUsed;
						resultItems.Add (pValue);
					}
				}

				value = new ValueEnumOperandValue<T> ((T)(object)words [0], resultItems);
			}

			wordsUsed = wordsUsedForParameters + 1;

			return true;
		}
	}

	public class IdScope : OperandType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = (Scope)words [0];
			wordsUsed = 1;

			return true;
		}
	}

	public class IdMemorySemantics : OperandType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = (MemorySemantics)words [0];
			wordsUsed = 1;

			return true;
		}
	}

	public class IdType : OperandType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = words [0];
			wordsUsed = 1;

			return true;
		}
	}

	public class IdResult : IdType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = new ObjectReference (words [0]);
			wordsUsed = 1;

			return true;
		}
	}

	public class IdResultType : IdType
	{
	}

	public class IdRef : IdType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = new ObjectReference (words [0]);
			wordsUsed = 1;

			return true;
		}
	}

	public class PairIdRefIdRef : OperandType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = new { Variable = new ObjectReference (words [0]), Parent = new ObjectReference (words [1]) };
			wordsUsed = 2;
			return true;
		}
	}

	public class PairIdRefLiteralInteger : OperandType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = new { Type = new ObjectReference (words [0]), Member = words [1] };
			wordsUsed = 2;
			return true;
		}
	}

	public class PairLiteralIntegerIdRef : OperandType
	{
		public override bool ReadValue (IList<uint> words, out object value, out int wordsUsed)
		{
			value = new { Selector = words [0], Label = new ObjectReference (words [1]) };
			wordsUsed = 2;
			return true;
		}
	}
}