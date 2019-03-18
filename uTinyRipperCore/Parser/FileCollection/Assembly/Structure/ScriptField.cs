using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Assembly
{
	public abstract class ScriptField : IScriptField
	{
		protected ScriptField(ScriptType type, bool isArray, string name)
		{
			if(type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if(string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(name);
			}

			Type = type;
			IsArray = isArray;
			Name = name;
		}

		protected ScriptField(ScriptField copy) :
			this(copy.Type, copy.IsArray, copy.Name)
		{
		}

		protected static bool IsCompilerGeneratedAttrribute(string @namespace, string name)
		{
			if (@namespace == ScriptType.CompilerServicesNamespace)
			{
				return name == ScriptType.CompilerGeneratedName;
			}
			return false;
		}

		protected static bool IsSerializeFieldAttrribute(string @namespace, string name)
		{
			if (@namespace == ScriptType.UnityEngineName)
			{
				return name == SerializeFieldName;
			}
			return false;
		}

		public abstract IScriptField CreateCopy();

		public void Read(AssetReader reader)
		{
			switch (Type.Type)
			{
				case PrimitiveType.Bool:
					if (IsArray)
					{
						Value = reader.ReadBooleanArray();
					}
					else
					{
						Value = reader.ReadBoolean();
					}
					reader.AlignStream(AlignType.Align4);
					break;

				case PrimitiveType.Byte:
					if (IsArray)
					{
						Value = reader.ReadByteArray();
					}
					else
					{
						Value = reader.ReadByte();
					}
					reader.AlignStream(AlignType.Align4);
					break;

				case PrimitiveType.Char:
					if (IsArray)
					{
						Value = reader.ReadCharArray();
					}
					else
					{
						Value = reader.ReadChar();
					}
					reader.AlignStream(AlignType.Align4);
					break;

				case PrimitiveType.Short:
					if (IsArray)
					{
						Value = reader.ReadInt16Array();
					}
					else
					{
						Value = reader.ReadInt16();
					}
					reader.AlignStream(AlignType.Align4);
					break;

				case PrimitiveType.UShort:
					if (IsArray)
					{
						Value = reader.ReadUInt16Array();
					}
					else
					{
						Value = reader.ReadUInt16();
					}
					reader.AlignStream(AlignType.Align4);
					break;

				case PrimitiveType.Int:
					if (IsArray)
					{
						Value = reader.ReadInt32Array();
					}
					else
					{
						Value = reader.ReadInt32();
					}
					break;

				case PrimitiveType.UInt:
					if (IsArray)
					{
						Value = reader.ReadUInt32Array();
					}
					else
					{
						Value = reader.ReadUInt32();
					}
					break;

				case PrimitiveType.Long:
					if (IsArray)
					{
						Value = reader.ReadInt64Array();
					}
					else
					{
						Value = reader.ReadInt64();
					}
					break;

				case PrimitiveType.ULong:
					if (IsArray)
					{
						Value = reader.ReadUInt64Array();
					}
					else
					{
						Value = reader.ReadUInt64();
					}
					break;

				case PrimitiveType.Single:
					if (IsArray)
					{
						Value = reader.ReadSingleArray();
					}
					else
					{
						Value = reader.ReadSingle();
					}
					break;

				case PrimitiveType.Double:
					if (IsArray)
					{
						Value = reader.ReadDoubleArray();
					}
					else
					{
						Value = reader.ReadDouble();
					}
					break;

				case PrimitiveType.String:
					if (IsArray)
					{
						Value = reader.ReadStringArray();
					}
					else
					{
						Value = reader.ReadString();
					}
					break;

				case PrimitiveType.Complex:
					if (IsArray)
					{
						int count = reader.ReadInt32();
						IScriptStructure[] structures = new IScriptStructure[count];
						for (int i = 0; i < count; i++)
						{
							IScriptStructure structure = Type.ComplexType.CreateCopy();
							structure.Read(reader);
							structures[i] = structure;
						}
						Value = structures;
					}
					else
					{
						IScriptStructure structure = Type.ComplexType.CreateCopy();
						structure.Read(reader);
						Value = structure;
					}
					break;

				default:
					throw new NotImplementedException($"Unknown {nameof(PrimitiveType)} '{Type.Type}'");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			if (IsArray)
			{
				if (Type.Type == PrimitiveType.Complex)
				{
					IEnumerable<IScriptStructure> structures = (IEnumerable<IScriptStructure>)Value;
					return structures.ExportYAML(container);
				}
				else
				{
					switch (Type.Type)
					{
						case PrimitiveType.Bool:
							{
								bool[] array = (bool[])Value;
								return array.ExportYAML();
							}
						case PrimitiveType.Char:
							{
								char[] array = (char[])Value;
								return array.ExportYAML();
							}
						case PrimitiveType.Byte:
							{
								byte[] array = (byte[])Value;
								return array.ExportYAML();
							}
						case PrimitiveType.Short:
							{
								short[] array = (short[])Value;
								return array.ExportYAML(true);
							}
						case PrimitiveType.UShort:
							{
								ushort[] array = (ushort[])Value;
								return array.ExportYAML(true);
							}
						case PrimitiveType.Int:
							{
								int[] array = (int[])Value;
								return array.ExportYAML(true);
							}
						case PrimitiveType.UInt:
							{
								uint[] array = (uint[])Value;
								return array.ExportYAML(true);
							}
						case PrimitiveType.Long:
							{
								long[] array = (long[])Value;
								return array.ExportYAML(true);
							}
						case PrimitiveType.ULong:
							{
								ulong[] array = (ulong[])Value;
								return array.ExportYAML(true);
							}
						case PrimitiveType.Single:
							{
								float[] array = (float[])Value;
								return array.ExportYAML();
							}
						case PrimitiveType.Double:
							{
								double[] array = (double[])Value;
								return array.ExportYAML();
							}
						case PrimitiveType.String:
							{
								string[] array = (string[])Value;
								return array.ExportYAML();
							}
						default:
							throw new NotSupportedException(Type.Type.ToString());
					}
				}
			}
			else
			{
				if (Type.Type == PrimitiveType.Complex)
				{
					IScriptStructure structure = (IScriptStructure)Value;
					return structure.ExportYAML(container);
				}
				else
				{
					switch (Type.Type)
					{
						case PrimitiveType.Bool:
							return new YAMLScalarNode((bool)Value);
						case PrimitiveType.Char:
							return new YAMLScalarNode((int)(char)Value);
						case PrimitiveType.Byte:
							return new YAMLScalarNode((byte)Value);
						case PrimitiveType.Short:
							return new YAMLScalarNode((short)Value);
						case PrimitiveType.UShort:
							return new YAMLScalarNode((ushort)Value);
						case PrimitiveType.Int:
							return new YAMLScalarNode((int)Value);
						case PrimitiveType.UInt:
							return new YAMLScalarNode((uint)Value);
						case PrimitiveType.Long:
							return new YAMLScalarNode((long)Value);
						case PrimitiveType.ULong:
							return new YAMLScalarNode((ulong)Value);
						case PrimitiveType.Single:
							return new YAMLScalarNode((float)Value);
						case PrimitiveType.Double:
							return new YAMLScalarNode((double)Value);
						case PrimitiveType.String:
							return new YAMLScalarNode((string)Value);
						default:
							throw new NotSupportedException(Type.Type.ToString());
					}
				}
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			if (Type.Type == PrimitiveType.Complex)
			{
				if (IsArray)
				{
					IEnumerable<IScriptStructure> structures = (IEnumerable<IScriptStructure>)Value;
					foreach (IScriptStructure structure in structures)
					{
						foreach (Object asset in structure.FetchDependencies(file, isLog))
						{
							yield return asset;
						}
					}
				}
				else
				{
					IScriptStructure structure = (IScriptStructure)Value;
					foreach (Object asset in structure.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
		}

		public override string ToString()
		{
			string arraySymb = IsArray ? "[]" : string.Empty;
			return $"{Type.ToString()}{arraySymb} {Name}";
		}

		public string Name { get; }
		public ScriptType Type { get; }
		public bool IsArray { get; }
		public object Value { get; private set; }
		
		private const string SerializeFieldName = "SerializeField";
	}
}
