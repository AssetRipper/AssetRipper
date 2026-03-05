using System.Diagnostics;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler.Blocks;

public sealed class SHDR : ShaderBlock
{
	public int majorVersion;
	public int minorVersion;
	public Type shaderType;
	public int instructionSize;
	public List<SHDRInstruction> shaderInstructions;

	public override string FourCC => "SHDR";

	private readonly ShaderBlock[] blocks;
	public SHDR(Stream stream, ShaderBlock[] blocks)
	{
		this.blocks = blocks;
		using BinaryReader reader = new BinaryReader(stream);
		short version = reader.ReadInt16();
		majorVersion = (version & 0x00f0) >> 4;
		minorVersion = version & 0x000f;
		shaderType = (Type)reader.ReadInt16();
		instructionSize = reader.ReadInt32();
		shaderInstructions = new List<SHDRInstruction>();
		while (reader.BaseStream.Position < instructionSize * 4)
		{
			SHDRInstruction inst = new SHDRInstruction(reader, this);
			shaderInstructions.Add(inst);
		}
	}
}
public class SHDRInstruction
{
	public int instData;
	public int length;
	public bool operandExtended;
	public Opcode opcode;

	public bool saturated;

	public int addrOffsetIU, addrOffsetIV, addrOffsetIW;
	public int typeX, typeY, typeZ, typeW;
	public ResourceDimension resDim;

	public int opcodeExtraData;
	public int operandIndex;
	public int operandType;
	public int operandComponents;

	public SHDRDeclData? declData;
	public List<SHDRInstructionOperand> operands;
	public SHDR shader;
	public SHDRInstruction(BinaryReader reader, SHDR shader)
	{
		this.shader = shader;
		long startPos = reader.BaseStream.Position;
		instData = reader.ReadInt32();
		length = (instData & 0x7f000000) >> 24;
		operandExtended = (instData & 0x80000000) >> 31 == 1;
		opcode = (Opcode)(instData & 0x00007ff);

		saturated = (instData & 0x00002000) != 0;

		bool extended = operandExtended;
		while (extended)
		{
			int extOpcodeToken = reader.ReadInt32();
			ExtendedOpcodeType extOpcodeType = (ExtendedOpcodeType)(extOpcodeToken & 0x0000003f);
			extended = (extOpcodeToken & 0x80000000) >> 31 == 1;

			switch (extOpcodeType)
			{
				case ExtendedOpcodeType.SampleControls:
					{
						addrOffsetIU = (extOpcodeToken & 0x1e00) >> 9;
						addrOffsetIV = (extOpcodeToken & 0x1e000) >> 13;
						addrOffsetIW = (extOpcodeToken & 0x1e0000) >> 17;
						break;
					}
				case ExtendedOpcodeType.ResourceReturnType:
					{
						typeX = (extOpcodeToken & 0x3c0) >> 6;
						typeY = (extOpcodeToken & 0x3c00) >> 10;
						typeZ = (extOpcodeToken & 0x3c000) >> 14;
						typeW = (extOpcodeToken & 0x3c0000) >> 18;
						break;
					}
				case ExtendedOpcodeType.ResourceDim:
					{
						resDim = (ResourceDimension)((extOpcodeToken & 0x000007C0) >> 6);
						break;
					}
			}
		}

		opcodeExtraData = (instData & 0xfff800) >> 11;
		operands = new List<SHDRInstructionOperand>();

		long endPos = startPos + length * 4;
		if (IsDeclaration(opcode))
		{
			reader.BaseStream.Position = startPos + 4;
			declData = new SHDRDeclData(reader, this, instData);

			reader.BaseStream.Position = startPos + length * 4;
		}
		else
		{
			while (reader.BaseStream.Position < endPos)
			{
				operands.Add(new SHDRInstructionOperand(reader));
			}
		}

		//for (int i = 0; i < GetOperandCount(opcode); i++)
		//{
		//    operands.Add(new SHDRInstructionOperand(reader));
		//}

		if (reader.BaseStream.Position > endPos && opcode != Opcode.customdata)
		{
			throw new Exception($"went over end pos ({opcode})");
		}

		if (opcode == Opcode.customdata)
		{
			Debug.Assert(declData != null);
			reader.BaseStream.Position = startPos + declData.customDataArray.Length * 16 + 8;
		}
		else
		{
			reader.BaseStream.Position = endPos;
		}
	}
	//DeocdeInstruction
	public static int GetOperandCount(Opcode opcode)
	{
		switch (opcode)
		{
			case Opcode.cut:
			case Opcode.emit:
			case Opcode.emit_then_cut:
			case Opcode.ret:
			case Opcode.loop:
			case Opcode.endloop:
			case Opcode.@break:
			case Opcode.@else:
			case Opcode.endif:
			case Opcode.@continue:
			case Opcode.@default:
			case Opcode.endswitch:
			case Opcode.nop:
			case Opcode.hs_control_point_phase:
			case Opcode.hs_fork_phase:
			case Opcode.hs_join_phase:
			case Opcode.dcl_hs_fork_phase_instance_count:
			case Opcode.sync:
			case Opcode.customdata: //variable size
			case Opcode.dcl_sampler:
			case Opcode.dcl_outputtopology:
			case Opcode.dcl_inputprimitive:
			case Opcode.dcl_maxout:
			case Opcode.dcl_tessellator_paritioning:
			case Opcode.dcl_tessellator_domain:
			case Opcode.dcl_tessellator_output_primitive:
			case Opcode.dcl_thread_group:
			case Opcode.dcl_input_ps_siv:
			case Opcode.dcl_output_sgv:
			case Opcode.dcl_temps:
			case Opcode.dcl_indexableTemp:
			case Opcode.dcl_globalFlags:
			case Opcode.dcl_interface:
			case Opcode.dcl_function_table:
			case Opcode.dcl_input_control_point_count:
			case Opcode.hs_decls:
			case Opcode.dcl_output_control_point_count:
			case Opcode.dcl_hs_max_tessfactor:
				return 0;
			case Opcode.emit_stream:
			case Opcode.cut_stream:
			case Opcode.emit_then_cut_stream:
			case Opcode.@case:
			case Opcode.@switch:
			case Opcode.label:
			case Opcode.fcall:
			case Opcode.dcl_resource:
			case Opcode.dcl_constantbuffer:
			case Opcode.dcl_indexrange:
			case Opcode.dcl_input:
			case Opcode.dcl_input_siv:
			case Opcode.dcl_input_ps:
			case Opcode.dcl_input_sgv:
			case Opcode.dcl_input_ps_sgv:
			case Opcode.dcl_output:
			case Opcode.dcl_output_siv:
			case Opcode.dcl_function_body:
			case Opcode.dcl_uav_raw:
			case Opcode.dcl_uav_structured:
			case Opcode.dcl_resource_structured:
			case Opcode.dcl_resource_raw:
			case Opcode.dcl_tgsm_structured:
			case Opcode.dcl_tgsm_raw:
			case Opcode.dcl_stream:
				return 1;
			case Opcode.mov:
			case Opcode.log:
			case Opcode.rsq:
			case Opcode.exp:
			case Opcode.sqrt:
			case Opcode.round_pi:
			case Opcode.round_ni:
			case Opcode.round_z:
			case Opcode.round_ne:
			case Opcode.frc:
			case Opcode.ftou:
			case Opcode.ftoi:
			case Opcode.utof:
			case Opcode.itof:
			case Opcode.ineg:
			case Opcode.imm_atomic_alloc:
			case Opcode.imm_atomic_consume:
			case Opcode.dmov:
			case Opcode.dtof:
			case Opcode.ftod:
			case Opcode.drcp:
			case Opcode.countbits:
			case Opcode.firstbit_hi:
			case Opcode.firstbit_lo:
			case Opcode.firstbit_shi:
			case Opcode.bfrev:
			case Opcode.f32tof16:
			case Opcode.f16tof32:
			case Opcode.rcp:
			case Opcode.not:
			case Opcode.deriv_rtx_coarse:
			case Opcode.deriv_rty_coarse:
			case Opcode.deriv_rtx_fine:
			case Opcode.deriv_rty_fine:
			case Opcode.deriv_rtx:
			case Opcode.deriv_rty:
			case Opcode.@if:
			case Opcode.breakc:
			case Opcode.callc:
			case Opcode.continuec:
			case Opcode.retc:
			case Opcode.discard:
			case Opcode.eval_centroid:
			case Opcode.dcl_uav_typed:
				return 2;
			case Opcode.sincos:
			case Opcode.min:
			case Opcode.umin:
			case Opcode.umax:
			case Opcode.imax:
			case Opcode.imin:
			case Opcode.max:
			case Opcode.mul:
			case Opcode.div:
			case Opcode.add:
			case Opcode.dp2:
			case Opcode.dp3:
			case Opcode.dp4:
			case Opcode.ne:
			case Opcode.or:
			case Opcode.lt:
			case Opcode.ieq:
			case Opcode.iadd:
			case Opcode.and:
			case Opcode.ge:
			case Opcode.ige:
			case Opcode.eq:
			case Opcode.ushr:
			case Opcode.ishl:
			case Opcode.ishr:
			case Opcode.ld:
			case Opcode.imul:
			case Opcode.ilt:
			case Opcode.ine:
			case Opcode.uge:
			case Opcode.ult:
			case Opcode.atomic_and:
			case Opcode.atomic_iadd:
			case Opcode.atomic_or:
			case Opcode.atomic_xor:
			case Opcode.atomic_imax:
			case Opcode.atomic_imin:
			case Opcode.atomic_umax:
			case Opcode.atomic_umin:
			case Opcode.dadd:
			case Opcode.dmax:
			case Opcode.dmin:
			case Opcode.dmul:
			case Opcode.deq:
			case Opcode.dge:
			case Opcode.dlt:
			case Opcode.dne:
			case Opcode.ddiv:
			case Opcode.xor:
			case Opcode.samplepos:
				return 3;
			case Opcode.mad:
			case Opcode.movc:
			case Opcode.imad:
			case Opcode.udiv:
			case Opcode.lod:
			case Opcode.sample:
			case Opcode.gather4:
			case Opcode.ldms:
			case Opcode.ubfe:
			case Opcode.ibfe:
			case Opcode.atomic_cmp_store:
			case Opcode.imm_atomic_iadd:
			case Opcode.imm_atomic_and:
			case Opcode.imm_atomic_or:
			case Opcode.imm_atomic_xor:
			case Opcode.imm_atomic_exch:
			case Opcode.imm_atomic_imax:
			case Opcode.imm_atomic_imin:
			case Opcode.imm_atomic_umax:
			case Opcode.imm_atomic_umin:
			case Opcode.dmovc:
			case Opcode.dfma:
			case Opcode.store_structured:
			case Opcode.ld_structured:
				return 4;
			case Opcode.gather4_po:
			case Opcode.sample_l:
			case Opcode.bfi:
			case Opcode.swapc:
			case Opcode.imm_atomic_cmp_exch:
			case Opcode.gather4_c:
			case Opcode.sample_c:
			case Opcode.sample_c_lz:
			case Opcode.sample_b:
				return 5;
			case Opcode.gather4_po_c:
			case Opcode.sample_d:
				return 6;
			default:
				return -1;
		}
	}
	public static bool IsDeclaration(Opcode opcode)
	{
		switch (opcode)
		{
			case Opcode.dcl_resource:
			case Opcode.dcl_constantbuffer:
			case Opcode.dcl_sampler:
			case Opcode.dcl_indexrange:
			case Opcode.dcl_outputtopology:
			case Opcode.dcl_inputprimitive:
			case Opcode.dcl_maxout:
			case Opcode.dcl_tessellator_paritioning:
			case Opcode.dcl_tessellator_domain:
			case Opcode.dcl_tessellator_output_primitive:
			case Opcode.dcl_thread_group:
			case Opcode.dcl_input:
			case Opcode.dcl_input_siv:
			case Opcode.dcl_input_ps:
			case Opcode.dcl_input_sgv:
			case Opcode.dcl_input_ps_sgv:
			case Opcode.dcl_input_ps_siv:
			case Opcode.dcl_output:
			case Opcode.dcl_output_sgv:
			case Opcode.dcl_output_siv:
			case Opcode.dcl_temps:
			case Opcode.dcl_indexableTemp:
			case Opcode.dcl_globalFlags:
			case Opcode.dcl_interface:
			case Opcode.dcl_function_body:
			case Opcode.dcl_function_table:
			case Opcode.dcl_input_control_point_count:
			case Opcode.hs_decls:
			case Opcode.dcl_output_control_point_count:
			case Opcode.hs_join_phase:
			case Opcode.hs_fork_phase:
			case Opcode.hs_control_point_phase:
			case Opcode.dcl_hs_fork_phase_instance_count:
			case Opcode.customdata:
			case Opcode.dcl_hs_max_tessfactor:
			case Opcode.dcl_uav_typed:
			case Opcode.dcl_uav_raw:
			case Opcode.dcl_uav_structured:
			case Opcode.dcl_resource_structured:
			case Opcode.dcl_resource_raw:
			case Opcode.dcl_tgsm_structured:
			case Opcode.dcl_tgsm_raw:
			case Opcode.dcl_stream:
				return true;
			default:
				return false;
		}
	}
}
public class SHDRInstructionOperand
{
	public Operand operand;
	public int indexDims;
	public int components;
	public int componentMode;
	public bool extended;
	public int extendedData;
	public int[] swizzle = Array.Empty<int>();
	public int[]? indexes;
	public double[] immValues;
	public int[] arraySizes;
	public SHDRInstructionOperand[] subOperands;
	public int registerNumber;
	public SHDRInstructionOperand(BinaryReader reader)
	{
		int operandData = reader.ReadInt32();
		operand = (Operand)((operandData & 0x000ff000) >> 12);
		indexDims = (operandData & 0x00300000) >> 20;
		int componentCode = operandData & 0x00000003;
		switch (componentCode)
		{
			case 0:
				components = 0;
				break;
			case 1:
				components = 1;
				break;
			case 2:
				components = 4;
				break;
		}
		extended = (unchecked((uint)operandData) & 0x80000000) >> 31 == 1;
		if (extended)
		{
			extendedData = reader.ReadInt32();
		}
		if (components == 4)
		{
			componentMode = (operandData & 0x0000000c) >> 2;
			if (componentMode == 0)
			{
				int mask = (operandData & 0x000000f0) >> 4;
				// Not swizzle but using it anyway
				int p = 0;
				// Count bits (pretty bad but works)
				for (int i = 0; i < 4; i++)
				{
					if ((mask >> i & 1) == 1)
					{
						p++;
					}
				}
				swizzle = new int[p];
				p = 0;
				for (int i = 0; i < 4; i++)
				{
					if ((mask >> i & 1) == 1)
					{
						swizzle[p] = i;
						p++;
					}
				}
			}
			else if (componentMode == 1)
			{
				swizzle = new int[4];
				for (int i = 0; i < 4; i++)
				{
					swizzle[i] = operandData >> 4 + 2 * (i & 3) & 3;
				}
			}
			else if (componentMode == 2)
			{
				swizzle = new int[1];
				swizzle[0] = (operandData & 0x00000030) >> 4;
			}
			else
			{
				throw new Exception($"component mode {componentMode} invalid");
			}
		}
		immValues = new double[components];
		if (operand == Operand.Immediate32)
		{
			for (int i = 0; i < components; i++)
			{
				immValues[i] = reader.ReadSingle();
			}
		}
		else if (operand == Operand.Immediate64)
		{
			for (int i = 0; i < components; i++)
			{
				immValues[i] = reader.ReadDouble();
			}
		}
		arraySizes = new int[indexDims];
		subOperands = new SHDRInstructionOperand[indexDims];
		for (int i = 0; i < indexDims; i++)
		{
			OperandIndex opIndex = (OperandIndex)((operandData & 3 << 22 + 3 * (i & 3)) >> 22 + 3 * (i & 3));
			switch (opIndex)
			{
				case OperandIndex.Immediate32:
					arraySizes[i] = reader.ReadInt32();
					break;
				case OperandIndex.IndexRelative:
					subOperands[i] = new SHDRInstructionOperand(reader);
					break;
				case OperandIndex.Immediate32Relative:
					registerNumber = reader.ReadInt32();
					subOperands[i] = new SHDRInstructionOperand(reader);
					break;
				default:
					throw new Exception("unsupported operand indexing type");
			}
		}
	}
}
public class SHDRDeclData
{
	public int globalFlags;
	public int numTemps;
	public ResourceDimension resourceDimension;
	public ConstantBufferType constantBufferType;
	public int samplerIndex;
	public SamplerMode samplerMode;
	public InterpolationMode interpolation;
	public PrimitiveTopology outputPrimitiveTopology;
	public Primitive inputPrimitive;
	public int maxOutputVertexCount;
	public TessDomain tessDomain;
	public TessPartitioning tessPartitioning;
	public TessOutputPrimitive tessOutPrim;
	public int[]? workGroupSize;
	public int hullPhaseInstanceInfo;
	public float maxTessFactor;
	public int indexRange;
	public float[][] customDataArray = Array.Empty<float[]>();

	public int interfaceId;
	public int interfaceFuncTableCount;
	public int interfaceArraySize;

	public int uavGloballyCoherentAccess;
	public int uavBufferSize;
	public byte uavCounter;
	public ResourceReturnType uavType;

	public string? nameToken;

	public int tgsmStride;
	public int tgsmCount;

	public SHDRInstructionOperand[] operands = Array.Empty<SHDRInstructionOperand>();

	private static readonly string[] nameTokens =
	[
		"undefined",
		"position",
		"clip_distance",
		"cull_distance",
		"rendertarget_array_index",
		"viewport_array_index",
		"vertex_id",
		"primitive_id",
		"instance_id",
		"is_front_face",
		"sampleIndex",
		"finalQuadUeq0EdgeTessFactor",
		"finalQuadVeq0EdgeTessFactor",
		"finalQuadUeq1EdgeTessFactor",
		"finalQuadVeq1EdgeTessFactor",
		"finalQuadUInsideTessFactor",
		"finalQuadVInsideTessFactor",
		"finalTriUeq0EdgeTessFactor",
		"finalTriVeq0EdgeTessFactor",
		"finalTriWeq0EdgeTessFactor",
		"finalTriInsideTessFactor",
		"finalLineDetailTessFactor",
		"finalLineDensityTessFactor"
	];

	public SHDRDeclData(BinaryReader reader, SHDRInstruction inst, int instData)
	{
		if (!SHDRInstruction.IsDeclaration(inst.opcode))
		{
			return;
		}

		int extraDataStart = SHDRInstruction.GetOperandCount(Opcode.dcl_indexrange) * 4;
		switch (inst.opcode)
		{
			case Opcode.dcl_resource:
				{
					resourceDimension = (ResourceDimension)((instData & 0x0000f800) >> 11);
					operands =
					[
					new SHDRInstructionOperand(reader)
					];
					break;
				}
			case Opcode.dcl_indexrange:
				{
					//reader.BaseStream.Position += extraDataStart;
					//indexRange = reader.ReadInt32();
					//if (inst.operands[0].operand == Operand.Input)
					//{
					//    int reg = inst.operands[0].arraySizes.Length;
					//    
					//}
					//else if (inst.operands[0].operand == Operand.Output)
					//{
					//
					//}
					break;
				}
			case Opcode.dcl_constantbuffer:
				{
					constantBufferType = (ConstantBufferType)((instData & 0x00fff800) >> 11);
					operands =
					[
					new SHDRInstructionOperand(reader)
					];
					break;
				}
			case Opcode.dcl_sampler:
				{
					reader.BaseStream.Position += 4;
					samplerIndex = reader.ReadInt32();
					samplerMode = (SamplerMode)((instData & 0x00fff800) >> 11);
					break;
				}
			case Opcode.dcl_outputtopology:
				{
					outputPrimitiveTopology = (PrimitiveTopology)((instData & 0x0001f800) >> 11);
					break;
				}
			case Opcode.dcl_inputprimitive:
				{
					inputPrimitive = (Primitive)((instData & 0x0001f800) >> 11);
					break;
				}
			case Opcode.dcl_maxout:
				{
					maxOutputVertexCount = reader.ReadInt32();//?
					break;
				}
			case Opcode.dcl_tessellator_paritioning:
				{
					tessPartitioning = (TessPartitioning)((instData & 0x00003800) >> 11);
					break;
				}
			case Opcode.dcl_tessellator_domain:
				{
					tessDomain = (TessDomain)((instData & 0x00001800) >> 11);
					break;
				}
			case Opcode.dcl_tessellator_output_primitive:
				{
					tessOutPrim = (TessOutputPrimitive)((instData & 0x00003800) >> 11);
					break;
				}
			case Opcode.dcl_thread_group:
				{
					workGroupSize =
					//?
					[
					reader.ReadInt32(),
					reader.ReadInt32(),
					reader.ReadInt32()
					];
					break;
				}
			case Opcode.dcl_input_siv:
				{
					if (inst.shader.shaderType == Type.Pixel)
					{
						interpolation = (InterpolationMode)((instData & 0x00007800) >> 11);
					}
					break;
				}
			case Opcode.dcl_input_ps:
				{
					interpolation = (InterpolationMode)((instData & 0x00007800) >> 11);
					break;
				}
			case Opcode.dcl_input_sgv:
			case Opcode.dcl_input_ps_sgv:
				{
					reader.BaseStream.Position += 8;
					nameToken = nameTokens[reader.ReadInt32()];
					//todo edit: done yet?
					break;
				}
			case Opcode.dcl_input_ps_siv:
				{
					interpolation = (InterpolationMode)((instData & 0x00007800) >> 11);
					break;
				}
			case Opcode.dcl_output_siv:
				{
					reader.BaseStream.Position += 8;
					nameToken = nameTokens[reader.ReadInt32()];
					//todo edit: done yet?
					break;
				}
			case Opcode.dcl_temps:
				{
					numTemps = reader.ReadInt32();
					break;
				}
			case Opcode.dcl_globalFlags:
				{
					globalFlags = (instData & 0x00fff800) >> 11;
					break;
				}
			case Opcode.dcl_interface:
				{
					interfaceId = reader.ReadInt32();
					reader.BaseStream.Position += 4;
					int tableInfo = reader.ReadInt32();
					interfaceFuncTableCount = tableInfo & 0x0000ffff;
					interfaceArraySize = (tableInfo & unchecked((int)0xffff0000)) >> 16;
					//todo
					break;
				}
			case Opcode.dcl_function_table:
				{
					//todo
					break;
				}
			case Opcode.dcl_output_control_point_count:
				{
					maxOutputVertexCount = reader.ReadInt32();
					break;
				}
			case Opcode.dcl_hs_fork_phase_instance_count:
				{
					hullPhaseInstanceInfo = reader.ReadInt32();
					break;
				}
			case Opcode.customdata:
				{
					int customDataOperandCount = (reader.ReadInt32() - 2) / 4;
					customDataArray = new float[customDataOperandCount][];
					for (int i = 0; i < customDataOperandCount; i++)
					{
						//how do we know which type? using float for now
						customDataArray[i] =
						[
						reader.ReadSingle(),
						reader.ReadSingle(),
						reader.ReadSingle(),
						reader.ReadSingle()
						];
					}
					break;
				}
			case Opcode.dcl_hs_max_tessfactor:
				{
					maxTessFactor = reader.ReadSingle();
					break;
				}
			case Opcode.dcl_uav_typed:
				{
					resourceDimension = (ResourceDimension)((instData & 0x0000f800) >> 11);
					uavGloballyCoherentAccess = instData & 0x00010000;
					uavCounter = 0;
					uavBufferSize = 0;
					reader.BaseStream.Position += 4;
					uavType = (ResourceReturnType)(reader.ReadInt32() & 0xF);
					break;
				}
			case Opcode.dcl_uav_raw:
				{
					uavGloballyCoherentAccess = instData & 0x00010000;
					uavCounter = 0;
					uavBufferSize = 0;
					break;
				}
			case Opcode.dcl_uav_structured:
				{
					uavGloballyCoherentAccess = instData & 0x00010000;
					uavCounter = 0;
					uavBufferSize = 0;
					//todo
					break;
				}
			case Opcode.dcl_tgsm_structured:
				{
					uavGloballyCoherentAccess = 0;
					reader.BaseStream.Position += 4;
					tgsmStride = reader.ReadInt32();
					tgsmCount = reader.ReadInt32();
					break;
				}
			case Opcode.dcl_tgsm_raw:
				{
					uavGloballyCoherentAccess = 0;
					reader.BaseStream.Position += 4;
					tgsmStride = 4;
					tgsmCount = reader.ReadInt32();
					break;
				}
		}
	}
}
