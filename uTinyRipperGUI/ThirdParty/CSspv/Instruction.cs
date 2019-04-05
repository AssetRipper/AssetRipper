using System;
using System.Collections.Generic;
using System.Text;

namespace SpirV
{
	public enum OperandQuantifier
	{
		// 1
		Default,
		// 0 or 1
		Optional,
		// 0+
		Varying
	}

	public class Operand
	{
		public string Name { get; }
		public OperandType Type { get; }
		public OperandQuantifier Quantifier { get; }
		
		public Operand(OperandType kind, string name, OperandQuantifier quantifier)
		{
			Name = name;
			Type = kind;
			Quantifier = quantifier;
		}
	}

	public class Instruction
	{
		public string Name { get; }

		public IList<Operand> Operands
		{
			get;
		}

		public Instruction (string name)
			: this (name, new List<Operand> ())
		{
		}

		public Instruction (string name, IList<Operand> operands)
		{
			Operands = operands;
			Name = name;
		}
	}
}
