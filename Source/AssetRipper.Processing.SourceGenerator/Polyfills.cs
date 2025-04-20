namespace AssetRipper.Processing.SourceGenerator;

/// <summary>
/// Contains source files for the polyfill assembly.
/// Some are hand-written, and others are copied from the .NET runtime.
/// </summary>
/// <remarks>
/// <seealso href="https://source.dot.net/"/>
/// </remarks>
internal static class Polyfills
{
	public static IEnumerable<string> Get() =>
	[
		Object,
		ValueType,
		Enum,
		Type,
		SByte,
		Byte,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,
		Single,
		Double,
		Void,
		Boolean,
		Char,
		String,
		Attribute,
		AttributeTargets,
		AttributeUsageAttribute,
		FlagsAttribute,
		AssemblyVersionAttribute,
		MethodImplAttributes,
		MethodCodeType,
		MethodImplOptions,
		MethodImplAttribute,
		TypeForwardedToAttribute,
		IsUnmanagedAttribute,
	];

	private const string Object = """
		namespace System
		{
			public class Object
			{
				public virtual string? ToString() => null;
				public virtual bool Equals(object? obj) => false;
				public virtual int GetHashCode() => 0;
				public Type GetType() => null;
			}
		}
		""";

	private const string ValueType = """
		namespace System
		{
			public abstract class ValueType
			{
			}
		}
		""";

	private const string Enum = """
		namespace System
		{
			public abstract class Enum : ValueType
			{
			}
		}
		""";

	private const string Type = """
		namespace System
		{
			public class Type
			{
				//public static Type GetTypeFromHandle(RuntimeTypeHandle handle) => null;
			}
		}
		""";

	private const string SByte = """
		namespace System
		{
			public readonly struct SByte
			{
				private readonly sbyte m_value;
			}
		}
		""";

	private const string Byte = """
		namespace System
		{
			public readonly struct Byte
			{
				private readonly byte m_value;
			}
		}
		""";

	private const string Int16 = """
		namespace System
		{
			public readonly struct Int16
			{
				private readonly short m_value;
			}
		}
		""";

	private const string UInt16 = """
		namespace System
		{
			public readonly struct UInt16
			{
				private readonly ushort m_value;
			}
		}
		""";

	private const string Int32 = """
		namespace System
		{
			public readonly struct Int32
			{
				private readonly int m_value;
			}
		}
		""";

	private const string UInt32 = """
		namespace System
		{
			public readonly struct UInt32
			{
				private readonly int m_value;
			}
		}
		""";

	private const string Int64 = """
		namespace System
		{
			public readonly struct Int64
			{
				private readonly long m_value;
			}
		}
		""";

	private const string UInt64 = """
		namespace System
		{
			public readonly struct UInt64
			{
				private readonly ulong m_value;
			}
		}
		""";

	private const string Single = """
		namespace System
		{
			public readonly struct Single
			{
				private readonly float m_value;
			}
		}
		""";

	private const string Double = """
		namespace System
		{
			public readonly struct Double
			{
				private readonly double m_value;
			}
		}
		""";

	private const string Void = """
		namespace System
		{
			public readonly struct Void
			{
			}
		}
		""";

	private const string Boolean = """
		namespace System
		{
			public readonly struct Boolean
			{
				private readonly bool m_value;
			}
		}
		""";

	private const string Char = """
		namespace System
		{
			public readonly struct Char
			{
				private readonly char m_value;
			}
		}
		""";

	private const string String = """
		namespace System
		{
			public sealed class String
			{
			}
		}
		""";

	private const string Attribute = """
		namespace System
		{
			public abstract class Attribute
			{
			}
		}
		""";

	private const string AttributeTargets = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////

		namespace System
		{
			// Enum used to indicate all the elements of the
			// VOS it is valid to attach this element to.
			[Flags]
			public enum AttributeTargets
			{
				Assembly = 0x0001,
				Module = 0x0002,
				Class = 0x0004,
				Struct = 0x0008,
				Enum = 0x0010,
				Constructor = 0x0020,
				Method = 0x0040,
				Property = 0x0080,
				Field = 0x0100,
				Event = 0x0200,
				Interface = 0x0400,
				Parameter = 0x0800,
				Delegate = 0x1000,
				ReturnValue = 0x2000,
				GenericParameter = 0x4000,

				All = Assembly | Module | Class | Struct | Enum | Constructor |
								Method | Property | Field | Event | Interface | Parameter |
								Delegate | ReturnValue | GenericParameter
			}
		}
		""";

	private const string AttributeUsageAttribute = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		namespace System
		{
			/// <summary>
			/// Specifies the usage of another attribute class.
			/// </summary>
			[AttributeUsage(AttributeTargets.Class, Inherited = true)]
			public sealed class AttributeUsageAttribute : Attribute
			{
				private readonly AttributeTargets _attributeTarget;
				private bool _allowMultiple;
				private bool _inherited;

				internal static readonly AttributeUsageAttribute Default = new AttributeUsageAttribute(AttributeTargets.All);

				public AttributeUsageAttribute(AttributeTargets validOn)
				{
					_attributeTarget = validOn;
					_inherited = true;
				}

				internal AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited)
				{
					_attributeTarget = validOn;
					_allowMultiple = allowMultiple;
					_inherited = inherited;
				}

				public AttributeTargets ValidOn => _attributeTarget;

				public bool AllowMultiple
				{
					get => _allowMultiple;
					set => _allowMultiple = value;
				}

				public bool Inherited
				{
					get => _inherited;
					set => _inherited = value;
				}
			}
		}
		""";

	private const string FlagsAttribute = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////

		namespace System
		{
			// Custom attribute to indicate that the enum
			// should be treated as a bitfield (or set of flags).
			// An IDE may use this information to provide a richer
			// development experience.
			[AttributeUsage(AttributeTargets.Enum, Inherited = false)]
			public class FlagsAttribute : Attribute
			{
				public FlagsAttribute()
				{
				}
			}
		}
		""";

	private const string AssemblyVersionAttribute = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		namespace System.Reflection
		{
			[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
			public sealed class AssemblyVersionAttribute : Attribute
			{
				public AssemblyVersionAttribute(string version)
				{
					Version = version;
				}

				public string Version { get; }
			}
		}
		""";

	private const string MethodImplAttributes = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		namespace System.Reflection
		{
			// This Enum matches the CorMethodImpl defined in CorHdr.h
			public enum MethodImplAttributes
			{
				// code impl mask
				CodeTypeMask = 0x0003,   // Flags about code type.
				IL = 0x0000,   // Method impl is IL.
				Native = 0x0001,   // Method impl is native.
				OPTIL = 0x0002,   // Method impl is OPTIL
				Runtime = 0x0003,   // Method impl is provided by the runtime.
									// end code impl mask

				// managed mask
				ManagedMask = 0x0004,   // Flags specifying whether the code is managed or unmanaged.
				Unmanaged = 0x0004,   // Method impl is unmanaged, otherwise managed.
				Managed = 0x0000,   // Method impl is managed.
									// end managed mask

				// implementation info and interop
				ForwardRef = 0x0010,   // Indicates method is not defined; used primarily in merge scenarios.
				PreserveSig = 0x0080,   // Indicates method sig is exported exactly as declared.

				InternalCall = 0x1000,   // Internal Call...

				Synchronized = 0x0020,   // Method is single threaded through the body.
				NoInlining = 0x0008,   // Method may not be inlined.
				AggressiveInlining = 0x0100,   // Method should be inlined if possible.
				NoOptimization = 0x0040,   // Method may not be optimized.
				AggressiveOptimization = 0x0200, // Method may contain hot code and should be aggressively optimized.

				MaxMethodImplVal = 0xffff,
			}
		}
		""";

	private const string MethodCodeType = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		using System.Reflection;

		namespace System.Runtime.CompilerServices
		{
			public enum MethodCodeType
			{
				IL = MethodImplAttributes.IL,
				Native = MethodImplAttributes.Native,
				OPTIL = MethodImplAttributes.OPTIL,
				Runtime = MethodImplAttributes.Runtime
			}
		}
		""";

	private const string MethodImplOptions = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		namespace System.Runtime.CompilerServices
		{
			// This Enum matches the miImpl flags defined in corhdr.h. It is used to specify
			// certain method properties.
			[Flags]
			public enum MethodImplOptions
			{
				Unmanaged = 0x0004,
				NoInlining = 0x0008,
				ForwardRef = 0x0010,
				Synchronized = 0x0020,
				NoOptimization = 0x0040,
				PreserveSig = 0x0080,
				AggressiveInlining = 0x0100,
				AggressiveOptimization = 0x0200,
				InternalCall = 0x1000
			}
		}
		""";

	private const string MethodImplAttribute = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		namespace System.Runtime.CompilerServices
		{
			// Custom attribute to specify additional method properties.
			[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
			public sealed class MethodImplAttribute : Attribute
			{
				public MethodCodeType MethodCodeType;

				public MethodImplAttribute(MethodImplOptions methodImplOptions)
				{
					Value = methodImplOptions;
				}

				public MethodImplAttribute(short value)
				{
					Value = (MethodImplOptions)value;
				}

				public MethodImplAttribute()
				{
				}

				public MethodImplOptions Value { get; }
			}
		}
		""";

	private const string TypeForwardedToAttribute = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		namespace System.Runtime.CompilerServices
		{
			[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
			public sealed class TypeForwardedToAttribute : Attribute
			{
				public TypeForwardedToAttribute(Type destination)
				{
					Destination = destination;
				}

				public Type Destination { get; }
			}
		}
		""";

	/// <summary>
	/// Modified version of the IsUnmanagedAttribute from the .NET runtime.
	/// References to System.ComponentModel have been removed because they're in System.dll instead of mscorlib.
	/// </summary>
	private const string IsUnmanagedAttribute = """
		// Licensed to the .NET Foundation under one or more agreements.
		// The .NET Foundation licenses this file to you under the MIT license.

		//using System.ComponentModel;

		namespace System.Runtime.CompilerServices
		{
			/// <summary>
			/// Reserved for use by a compiler for tracking metadata.
			/// This attribute should not be used by developers in source code.
			/// </summary>
			//[EditorBrowsable(EditorBrowsableState.Never)]
			[AttributeUsage(AttributeTargets.All)]
			public sealed class IsUnmanagedAttribute : Attribute
			{
				/// <summary>Initializes the attribute.</summary>
				public IsUnmanagedAttribute()
				{
				}
			}
		}
		""";
}
