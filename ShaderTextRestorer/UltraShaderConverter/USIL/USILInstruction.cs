using System.Collections.Generic;
using System.Text;

namespace ShaderLabConvert
{
	public class USILInstruction
	{
		public USILInstructionType instructionType;
		public USILOperand destOperand;
		public List<USILOperand> srcOperands;
		public bool saturate;
		public bool commented;

		public bool isIntVariant;
		public bool isIntUnsigned;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(instructionType.ToString());

			sb.Append(' ');

			if (saturate)
			{
				sb.Append("saturate(");
			}

			if (destOperand != null)
			{
				sb.Append(destOperand.ToString());

				if (srcOperands.Count > 0)
				{
					sb.Append(", ");
				}
			}

			sb.Append(string.Join(", ", srcOperands));

			if (saturate)
			{
				sb.Append(')');
			}

			return sb.ToString();
		}

		// todo: all of them
		public bool IsComparisonType()
		{
			switch (instructionType)
			{
				case USILInstructionType.Equal:
				case USILInstructionType.NotEqual:
				case USILInstructionType.LessThan:
				case USILInstructionType.LessThanOrEqual:
				case USILInstructionType.GreaterThan:
				case USILInstructionType.GreaterThanOrEqual:
					return true;
				default:
					return false;
			}
		}

		public bool IsSampleType()
		{
			switch (instructionType)
			{
				case USILInstructionType.Sample:
				case USILInstructionType.SampleComparison:
				case USILInstructionType.SampleLOD:
				case USILInstructionType.SampleLODBias:
				case USILInstructionType.SampleComparisonLODZero:
				case USILInstructionType.SampleDerivative:
					return true;
				default:
					return false;
			}
		}
	}
}
