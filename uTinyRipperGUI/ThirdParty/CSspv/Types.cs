using System;
using System.Collections.Generic;
using System.Text;

namespace SpirV
{
	public class Type
	{
	}

	public class VoidType : Type
	{
		public override string ToString ()
		{
			return "void";
		}
	}

	public class ScalarType : Type
	{

	}

	public class BoolType : ScalarType
	{
		public override string ToString ()
		{
			return "bool";
		}
	}

	public class IntegerType : ScalarType
	{
		public IntegerType (int width, bool signed)
		{
			Width = width;
			Signed = signed;
		}

		public int Width { get; }
		public bool Signed { get; }

		public override string ToString ()
		{
			if (Signed) {
				return $"i{Width}";
			} else {
				return $"u{Width}";
			}
		}
	}

	public class FloatingPointType : ScalarType
	{
		public FloatingPointType (int width)
		{
			Width = width;
		}

		public int Width { get; }

		public override string ToString ()
		{
			return $"f{Width}";
		}
	}

	public class VectorType : Type
	{
		public VectorType (ScalarType scalarType, int componentCount)
		{
			ComponentType = scalarType;
			ComponentCount = componentCount;
		}

		public ScalarType ComponentType { get; }
		public int ComponentCount { get; }

		public override string ToString ()
		{
			return $"{ComponentType}_{ComponentCount}";
		}
	}

	public class MatrixType : Type
	{
		public MatrixType (VectorType vectorType, int columnCount)
		{
			ColumnType = vectorType;
			ColumnCount = columnCount;
		}

		public VectorType ColumnType { get; }
		public int ColumnCount { get; }
		public int RowCount { get { return ColumnType.ComponentCount; } }

		public override string ToString ()
		{
			return $"{ColumnType}x{ColumnCount}";
		}
	}

	public class ImageType : Type
	{
		public ImageType (Type sampledType, Dim dim, int depth,
			bool isArray, bool isMultisampled, int sampleCount,
			ImageFormat imageFormat, AccessQualifier accessQualifier)
		{
			SampledType = sampledType;
			Dim = dim;
			Depth = depth;
			IsArray = isArray;
			IsMultisampled = isMultisampled;
			SampleCount = sampleCount;
			Format = imageFormat;
			AccessQualifier = accessQualifier;
		}

		public Type SampledType { get; }
		public Dim Dim { get; }
		public int Depth { get; }
		public bool IsArray { get; }
		public bool IsMultisampled { get; }
		public int SampleCount { get; }
		public ImageFormat Format { get; }
		public AccessQualifier AccessQualifier { get; }

		public override string ToString ()
		{
			var sb = new StringBuilder ();
			switch (AccessQualifier) {
				case AccessQualifier.ReadWrite:
					sb.Append ("read_write "); break;
				case AccessQualifier.WriteOnly:
					sb.Append ("write_only "); break;
				case AccessQualifier.ReadOnly:
					sb.Append ("read_only "); break;
			}

			sb.Append ("Texture");
			switch (Dim) {
				case Dim.Dim1D: sb.Append ("1D"); break;
				case Dim.Dim2D: sb.Append ("2D"); break;
				case Dim.Dim3D: sb.Append ("3D"); break;
				case Dim.Cube: sb.Append ("Cube"); break;
			}

			if (IsMultisampled) {
				sb.Append ("MS");
			}

			if (IsArray) {
				sb.Append ("Array");
			}

			return sb.ToString ();
		}
	}

	public class SamplerType : Type
	{
		public override string ToString ()
		{
			return "sampler";
		}
	}

	public class SampledImageType : Type
	{
		public SampledImageType (ImageType imageType)
		{
			ImageType = imageType;
		}
		
		public ImageType ImageType { get; }

		public override string ToString ()
		{
			return $"{ImageType}Sampled";
		}
	}

	public class ArrayType : Type
	{
		public ArrayType (Type elementType, int elementCount)
		{
			ElementType = elementType;
			ElementCount = elementCount;
		}

		public int ElementCount { get; }
		public Type ElementType { get; }

		public override string ToString ()
		{
			return $"{ElementType}[{ElementCount}]";
		}
	}

	public class RuntimeArrayType : Type
	{
		public RuntimeArrayType (Type elementType)
		{
			ElementType = elementType;
		}

		public Type ElementType { get; }
	}

	public class StructType : Type
	{
		public StructType (IReadOnlyList<Type> memberTypes)
		{
			MemberTypes = memberTypes;
			memberNames_ = new List<string> ();

			for (int i = 0; i < memberTypes.Count; ++i) {
				memberNames_.Add (String.Empty);
			}
		}

		private List<string> memberNames_;

		public IReadOnlyList<Type> MemberTypes { get; }
		public IReadOnlyList<string> MemberNames { get { return memberNames_; } }

		public void SetMemberName (uint member, string name)
		{
			memberNames_ [(int)member] = name;
		}

		public override string ToString ()
		{
			var sb = new StringBuilder ();

			sb.Append ("struct {");
			for(int i = 0; i <  MemberTypes.Count; ++i) {
				var memberType = MemberTypes [i];
				sb.Append (memberType.ToString ());

				if (! string.IsNullOrEmpty (memberNames_ [i])) {
					sb.Append (" ");
					sb.Append (MemberNames [i]);
				}

				sb.Append (";");
				if (i < (MemberTypes.Count - 1)) {
					sb.Append (" ");
				}
			}
			sb.Append ("}");

			return sb.ToString ();
		}
	}

	public class OpaqueType : Type
	{
	}

	public class PointerType : Type
	{
		public StorageClass StorageClass { get; }
		public Type Type { get; private set; }

		public PointerType (StorageClass storageClass, Type type)
		{
			StorageClass = storageClass;
			Type = type;
		}

		public PointerType (StorageClass storageClass)
		{
			StorageClass = storageClass;
		}

		public void ResolveForwardReference (Type t)
		{
			Type = t;
		}

		public override string ToString ()
		{
			if (Type == null) {
				return $"{StorageClass} *";
			} else {
				return $"{StorageClass} {Type}*";
			}
		}
	}

	public class FunctionType : Type
	{
		public Type ReturnType { get; }
		public IReadOnlyList<Type> ParameterTypes { get { return parameterTypes_; } }

		private readonly List<Type> parameterTypes_ = new List<Type> ();

		public FunctionType (Type returnType, List<Type> parameterTypes)
		{
			ReturnType = returnType;
			parameterTypes_ = parameterTypes;
		}
	}

	public class EventType : Type
	{
	}

	public class DeviceEventType : Type
	{
	}

	public class ReserveIdType : Type
	{
	}

	public class QueueType : Type
	{
	}

	public class PipeType : Type
	{
	}

	public class PipeStorage : Type
	{
	}

	public class NamedBarrier : Type
	{
	}
}
