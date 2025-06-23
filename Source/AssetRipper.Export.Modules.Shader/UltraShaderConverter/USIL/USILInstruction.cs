using System.Text;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;

public class USILInstruction
{
	public USILInstructionType instructionType { get; set; }
	public USILOperand? destOperand { get; set; }
	public required List<USILOperand> srcOperands { get; set; }
	public bool saturate { get; set; }
	public bool commented { get; set; }

	public bool isIntVariant { get; set; }
	public bool isIntUnsigned { get; set; }

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
