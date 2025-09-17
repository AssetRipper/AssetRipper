using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.DirectX;
using System.Globalization;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;

public class USILOperand
{
	public USILOperandType operandType;

	public int[] immValueInt = Array.Empty<int>();
	public float[] immValueFloat = Array.Empty<float>();
	public bool immIsInt; // useless

	public bool absoluteValue;
	public bool negative;
	public bool transposeMatrix;

	public int registerIndex;
	public int arrayIndex;
	public USILOperand? arrayRelative;

	public string? metadataName;
	public bool metadataNameAssigned;
	public bool metadataNameWithArray;

	public string comment = string.Empty;

	public USILOperand[] children = Array.Empty<USILOperand>();

	public int[] mask;
	public bool displayMask;

	public USILOperand()
	{
		operandType = USILOperandType.None;

		immIsInt = false;

		absoluteValue = false;
		negative = false;
		transposeMatrix = false;

		registerIndex = 0;
		arrayIndex = 0;
		arrayRelative = null;

		metadataName = null;
		metadataNameAssigned = false;
		metadataNameWithArray = false;

		mask = Array.Empty<int>();
		displayMask = true;
	}

	public USILOperand(USILOperand original)
	{
		operandType = original.operandType;

		immValueInt = original.immValueInt;
		immValueFloat = original.immValueFloat;
		immIsInt = original.immIsInt;

		absoluteValue = original.absoluteValue;
		negative = original.negative;
		transposeMatrix = original.transposeMatrix;

		registerIndex = original.registerIndex;
		arrayIndex = original.arrayIndex;

		if (original.arrayRelative != null)
		{
			arrayRelative = new USILOperand(original.arrayRelative);
		}
		else
		{
			arrayRelative = null;
		}

		metadataName = original.metadataName;
		metadataNameAssigned = original.metadataNameAssigned;
		metadataNameWithArray = original.metadataNameWithArray;

		comment = original.comment;

		children = new USILOperand[original.children.Length];
		for (int i = 0; i < original.children.Length; i++)
		{
			children[i] = new USILOperand(original.children[i]);
		}

		mask = original.mask.ToArray();
		displayMask = original.displayMask;
	}

	public USILOperand(int value)
	{
		operandType = USILOperandType.ImmediateInt;
		immValueInt = [value];
		immIsInt = true;
		mask = [0];
	}

	public USILOperand(float value)
	{
		operandType = USILOperandType.ImmediateFloat;
		immValueFloat = [value];
		immIsInt = false;
		mask = [0];
	}

	public int GetValueCount()
	{
		switch (operandType)
		{
			case USILOperandType.ImmediateFloat:
				return immValueFloat.Length;
			case USILOperandType.ImmediateInt:
				return immValueInt.Length;
			case USILOperandType.Multiple:
				int multipleSum = 0;
				foreach (USILOperand operand in children)
				{
					multipleSum += operand.GetValueCount();
				}
				return multipleSum;
			default:
				return mask.Length;
		}
	}

	public override string ToString()
	{
		return ToString(false);
	}

	public string ToString(bool forceHideMask)
	{
		string prefix = "";
		string body = "";
		string suffix = "";

		bool displayMaskOverride = displayMask;
		if (forceHideMask)
		{
			displayMaskOverride = false;
		}

		if (!metadataNameAssigned)
		{
			prefix = GetTypeShortForm(operandType);
		}

		if (absoluteValue)
		{
			prefix = $"abs({prefix}";
		}

		if (negative)
		{
			prefix = $"-{prefix}";
		}

		if (metadataNameAssigned)
		{
			body = metadataName ?? "";
		}
		else
		{
			switch (operandType)
			{
				case USILOperandType.None:
					{
						body = "none";
						break;
					}
				case USILOperandType.Null:
					{
						body = "null";
						break;
					}
				case USILOperandType.Comment:
					{
						body = comment;
						break;
					}
				case USILOperandType.TempRegister:
				case USILOperandType.InputRegister:
				case USILOperandType.OutputRegister:
				case USILOperandType.ResourceRegister:
				case USILOperandType.SamplerRegister:
				case USILOperandType.Sampler2D:
				case USILOperandType.Sampler3D:
				case USILOperandType.SamplerCube:
					{
						body = $"{registerIndex}";
						break;
					}
				case USILOperandType.IndexableTempRegister:
					{
						body = $"{registerIndex}[{arrayIndex}]";
						break;
					}
				case USILOperandType.ConstantBuffer:
				case USILOperandType.Matrix:
					{
						body = $"{registerIndex}";
						break;
					}
				case USILOperandType.ImmediateConstantBuffer:
					{
						body = "";
						break;
					}
				case USILOperandType.ImmediateInt:
					{
						if (immValueInt.Length == 1)
						{
							body = $"{immValueInt[0]}";
						}
						else //if (immValueInt.Length > 1)
						{
							body += $"int{immValueInt.Length}(";
							for (int i = 0; i < immValueInt.Length; i++)
							{
								if (i != immValueInt.Length - 1)
								{
									body += $"{immValueInt[i]}, ";
								}
								else
								{
									body += $"{immValueInt[i]}";
								}
							}
							body += ")";
						}
						break;
					}
				case USILOperandType.ImmediateFloat:
					{
						if (immValueFloat.Length == 1)
						{
							// todo: check if number can't possibly be expressed as float and write in hex.
							// todo: float precision isn't correct atm. add precision check somewhere.
							body = $"{immValueFloat[0].ToString("0.0######", CultureInfo.InvariantCulture)}";
						}
						else //if (immValueFloat.Length > 1)
						{
							// todo: if all numbers are the same and it matches the mask, use it only once
							body += $"float{immValueFloat.Length}(";
							for (int i = 0; i < immValueFloat.Length; i++)
							{
								if (i != immValueFloat.Length - 1)
								{
									body += $"{immValueFloat[i].ToString("0.0######", CultureInfo.InvariantCulture)}, ";
								}
								else
								{
									body += $"{immValueFloat[i].ToString("0.0######", CultureInfo.InvariantCulture)}";
								}
							}
							body += ")";
						}
						break;
					}
				case USILOperandType.Multiple:
					{
						body += $"float{GetValueCount()}({string.Join(", ", children.ToList())})";
						break;
					}

				default:
					{
						if (DXShaderNamingUtils.HasSpecialInputOutputName(operandType))
						{
							body = DXShaderNamingUtils.GetSpecialInputOutputName(operandType);
						}
						break;
					}
			}
		}

		if (!metadataNameAssigned || metadataNameWithArray)
		{
			switch (operandType)
			{
				case USILOperandType.ConstantBuffer:
				case USILOperandType.Matrix:
					{
						if (arrayRelative != null)
						{
							if (arrayIndex == 0)
							{
								body += $"[{arrayRelative}]";
							}
							else
							{
								body += $"[{arrayRelative} + {arrayIndex}]";
							}
						}
						else
						{
							body += $"[{arrayIndex}]";
						}
						break;
					}
				case USILOperandType.ImmediateConstantBuffer:
					{
						body += $"[{arrayRelative} + {arrayIndex}]";
						break;
					}
			}
		}

		if (operandType != USILOperandType.ImmediateFloat &&
			operandType != USILOperandType.ImmediateInt &&
			operandType != USILOperandType.Multiple &&
			!DXShaderNamingUtils.HasSpecialInputOutputName(operandType) &&
			displayMaskOverride)
		{
			if (mask.Length > 0)
			{
				suffix += ".";
			}

			if (operandType == USILOperandType.Matrix)
			{
				string[] charArray = transposeMatrix ? USILConstants.TMATRIX_MASK_CHARS : USILConstants.MATRIX_MASK_CHARS;
				for (int i = 0; i < mask.Length; i++)
				{
					suffix += charArray[arrayIndex * 4 + mask[i]];
				}
			}
			else
			{
				for (int i = 0; i < mask.Length; i++)
				{
					suffix += USILConstants.MASK_CHARS[mask[i]];
				}
			}

			if (suffix == ".xyzw")
			{
				suffix = "";
			}
		}

		if (absoluteValue)
		{
			suffix = $"{suffix})";
		}

		return $"{prefix}{body}{suffix}";
	}

	public static string GetTypeShortForm(USILOperandType operandType)
	{
		return operandType switch
		{
			USILOperandType.TempRegister => "tmp",
			USILOperandType.IndexableTempRegister => "xtmp",
			USILOperandType.InputRegister => "in",
			USILOperandType.OutputRegister => "out",
			USILOperandType.ResourceRegister => "rsc",
			USILOperandType.SamplerRegister => "smp",
			USILOperandType.ConstantBuffer => "cb",
			USILOperandType.ImmediateConstantBuffer => "icb",
			_ => ""
		};
	}
}
