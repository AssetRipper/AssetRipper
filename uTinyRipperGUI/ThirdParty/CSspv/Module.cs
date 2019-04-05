using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace SpirV
{
	public class Module
	{
		public Module (ModuleHeader header, List<ParsedInstruction> instructions)
		{
			Header = header;
			instructions_ = instructions;

			Read (instructions_, objects_);
		}

		private static HashSet<string> debugInstructions_ = new HashSet<string>
		{
			"OpSourceContinued",
			"OpSource",
			"OpSourceExtension",
			"OpName",
			"OpMemberName",
			"OpString",
			"OpLine",
			"OpNoLine",
			"OpModuleProcessed"
		};

		public static bool IsDebugInstruction (ParsedInstruction instruction)
		{
			return debugInstructions_.Contains (instruction.Instruction.Name);
		}

		private static void Read (IList<ParsedInstruction> instructions,
			Dictionary<uint, ParsedInstruction> objects)
		{
			// Debug instructions can be only processed after everything
			// else has been parsed, as they may reference types which haven't
			// been seen in the file yet
			var debugInstructions = new List<ParsedInstruction> ();

			// Entry points contain forward references
			// Those need to be resolved afterwards
			var entryPoints = new List<ParsedInstruction> ();
			
			foreach (var instruction in instructions) {
				if (IsDebugInstruction (instruction)) {
					debugInstructions.Add (instruction);
					continue;
				}

				if (instruction.Instruction is OpEntryPoint) {
					entryPoints.Add (instruction);
					continue;
				}

				if (instruction.Instruction.Name.StartsWith ("OpType")) {
					ProcessTypeInstruction (instruction, objects);
				}

				instruction.ResolveResultType (objects);

				if (instruction.HasResult) {
					objects[instruction.ResultId] = instruction;
				}

				switch (instruction.Instruction) {
					// Constants require that the result type has been resolved
					case OpSpecConstant sc:
					case OpConstant oc: {
							var t = instruction.ResultType;
							System.Diagnostics.Debug.Assert (t != null);
							System.Diagnostics.Debug.Assert (t is ScalarType);
							
							var constant = ConvertConstant (
								instruction.ResultType as ScalarType,
								instruction.Words.Skip (3).ToList ());
							instruction.Operands[2].Value = constant;
							instruction.Value = constant;
							
							break;
						}
				}
			}

			foreach (var instruction in debugInstructions) {
				switch (instruction.Instruction) {
					case OpMemberName mn: {
							var t = objects[instruction.Words[1]].ResultType as StructType;

							System.Diagnostics.Debug.Assert (t != null);

							t.SetMemberName ((uint)instruction.Operands[1].Value,
								instruction.Operands[2].Value as string);
							break;
						}
					case OpName n: {
							// We skip naming objects we don't know about
							var t = objects[instruction.Words[1]];

							t.Name = instruction.Operands[1].Value as string;

							break;
						}
				}
			}

			foreach (var instruction in instructions) {
				instruction.ResolveReferences (objects);
			}
		}

		public static Module ReadFrom (System.IO.Stream stream)
		{
			var br = new System.IO.BinaryReader (stream);
			var reader = new Reader (br);

			var versionNumber = reader.ReadWord ();
			var version = new Version (
				(int)(versionNumber >> 16),
				(int)((versionNumber >> 8) & 0xFF));

			var generatorMagicNumber = reader.ReadWord ();
			var generatorToolId = (int)(generatorMagicNumber >> 16);

			string generatorVendor = "unknown";
			string generatorName = null;

			if (SpirV.Meta.Tools.ContainsKey (generatorToolId)) {
				var toolInfo = SpirV.Meta.Tools[generatorToolId];

				generatorVendor = toolInfo.Vendor;

				if (toolInfo.Name != null) {
					generatorName = toolInfo.Name;
				}
			}

			// Read header
			var header = new ModuleHeader
			{
				Version = version,
				GeneratorName = generatorName,
				GeneratorVendor = generatorVendor,
				GeneratorVersion = (int)(generatorMagicNumber & 0xFFFF),
				Bound = reader.ReadWord (),
				Reserved = reader.ReadWord ()
			};

			var instructions = new List<ParsedInstruction> ();

			try {
				while (true) {
					var instructionStart = reader.ReadWord ();
					var wordCount = (ushort)(instructionStart >> 16);
					var opCode = (int)(instructionStart & 0xFFFF);

					List<uint> words = new List<uint> ()
					{
						instructionStart
					};

					for (ushort i = 0; i < wordCount - 1; ++i) {
						words.Add (reader.ReadWord ());
					}

					instructions.Add (new ParsedInstruction (opCode, words));
				}
			} catch (System.IO.EndOfStreamException) {
			}

			return new Module (header, instructions);
		}

		/// <summary>
		/// Collect types from OpType* instructions
		/// </summary>
		private static void ProcessTypeInstruction (ParsedInstruction i,
			Dictionary<uint, ParsedInstruction> objects)
		{
			switch (i.Instruction) {
				case OpTypeInt t: {
						i.ResultType = new IntegerType (
							(int)i.Words[2],
							i.Words[3] == 1u);
					}
					break;
				case OpTypeFloat t: {
						i.ResultType = new FloatingPointType (
							(int)i.Words[2]);
					}
					break;
				case OpTypeVector t: {
						i.ResultType = new VectorType (
							objects[i.Words[2]].ResultType as ScalarType,
							(int)i.Words[3]);
					}
					break;
				case OpTypeMatrix t: {
						i.ResultType = new MatrixType (
							objects[i.Words[2]].ResultType as VectorType,
							(int)i.Words[3]);
					}
					break;
				case OpTypeArray t: {
						var constant = objects[i.Words[3]].Value;
						int size = 0;

						switch (constant) {
							case UInt16 u16: size = u16; break;
							case UInt32 u32: size = (int)u32; break;
							case UInt64 u64: size = (int)u64; break;
							case Int16 i16: size = i16; break;
							case Int32 i32: size = i32; break;
							case Int64 i64: size = (int)i64; break;
						}

						i.ResultType = new ArrayType (
							objects[i.Words[2]].ResultType,
							size);
					}
					break;
				case OpTypeRuntimeArray t: {
						i.ResultType = new RuntimeArrayType (
							objects[i.Words[2]].ResultType as Type);
					}
					break;
				case OpTypeBool t: {
						i.ResultType = new BoolType ();
					}
					break;
				case OpTypeOpaque t: {
						i.ResultType = new OpaqueType ();
					}
					break;
				case OpTypeVoid t: {
						i.ResultType = new VoidType ();
					}
					break;
				case OpTypeImage t: {
						var sampledType = objects[i.Operands[1].GetId ()].ResultType;
						var dim = i.Operands[2].GetSingleEnumValue<Dim> ();
						var depth = (uint)i.Operands[3].Value;
						var isArray = (uint)i.Operands[4].Value != 0;
						var isMultiSampled = (uint)i.Operands[5].Value != 0;
						var sampled = (uint)i.Operands[6].Value;

						var imageFormat = i.Operands[7].GetSingleEnumValue<ImageFormat> ();

						i.ResultType = new ImageType (sampledType,
							dim,
							(int)depth, isArray, isMultiSampled,
							(int)sampled, imageFormat,
							i.Operands.Count > 8 ?
							i.Operands[8].GetSingleEnumValue<AccessQualifier> () : AccessQualifier.ReadOnly);
					}
					break;
				case OpTypeSampler st: {
						i.ResultType = new SamplerType ();
						break;
					}
				case OpTypeSampledImage t: {
						i.ResultType = new SampledImageType (
							objects[i.Words[2]].ResultType as ImageType
						);
					}
					break;
				case OpTypeFunction t: {
						var parameterTypes = new List<Type> ();
						for (int j = 3; j < i.Words.Count; ++j) {
							parameterTypes.Add (objects[i.Words[j]].ResultType);
						}
						i.ResultType = new FunctionType (objects[i.Words[2]].ResultType,
							parameterTypes);
					}
					break;
				case OpTypeForwardPointer t: {
						// We create a normal pointer, but with unspecified type
						// This will get resolved later on
						i.ResultType = new PointerType ((StorageClass)i.Words[2]);
					}
					break;
				case OpTypePointer t: {
						if (objects.ContainsKey (i.Words[1])) {
							// If there is something present, it must have been
							// a forward reference. The storage type must
							// match
							var pt = i.ResultType as PointerType;
							Debug.Assert (pt != null);
							Debug.Assert (pt.StorageClass == (StorageClass)i.Words[2]);

							pt.ResolveForwardReference (objects[i.Words[3]].ResultType);
						} else {
							i.ResultType = new PointerType (
								(StorageClass)i.Words[2],
								objects[i.Words[3]].ResultType
								);
						}
					}
					break;
				case OpTypeStruct t: {
						var memberTypes = new List<Type> ();
						for (int j = 2; j < i.Words.Count; ++j) {
							memberTypes.Add (objects[i.Words[j]].ResultType);
						}

						i.ResultType = new StructType (memberTypes);
					}
					break;
			}
		}

		private static object ConvertConstant (ScalarType type,
			IReadOnlyList<uint> words)
		{
			byte[] bytes = new byte[words.Count * 4];

			for (int i = 0; i < words.Count; ++i) {
				BitConverter.GetBytes (words[i]).CopyTo (bytes, i * 4);
			}

			switch (type) {
				case IntegerType i: {
						if (i.Signed) {
							if (i.Width == 16) {
								///TODO ToInt16?
								return (short)BitConverter.ToInt32 (bytes, 0);
							} else if (i.Width == 32) {
								return BitConverter.ToInt32 (bytes, 0);
							} else if (i.Width == 64) {
								return BitConverter.ToInt64 (bytes, 0);
							}
						} else {
							if (i.Width == 16) {
								return (ushort)words[0];
							} else if (i.Width == 32) {
								return words[0];
							} else if (i.Width == 64) {
								return BitConverter.ToUInt64 (bytes, 0);
							}
						}

						throw new Exception ("Cannot construct integer literal.");
					}

				case FloatingPointType f: {
						if (f.Width == 32) {
							return BitConverter.ToSingle (bytes, 0);
						} else if (f.Width == 64) {
							return BitConverter.ToDouble (bytes, 0);
						} else {
							throw new Exception ("Cannot construct floating point literal.");
						}
					}
			}

			return null;
		}

		public ModuleHeader Header { get; }
		public IReadOnlyList<ParsedInstruction> Instructions { get { return instructions_; } }

		private readonly List<ParsedInstruction> instructions_;

		private readonly Dictionary<uint, ParsedInstruction> objects_ = new Dictionary<uint, ParsedInstruction> ();
	}
}
