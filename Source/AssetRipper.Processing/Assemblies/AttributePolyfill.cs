namespace AssetRipper.Processing.Assemblies;

internal readonly record struct AttributePolyfill(string Namespace, string Name, string Code)
{
	private const string AttributeCode = """
		namespace System
		{
			public abstract class Attribute
			{
			}
		}
		""";
	public static AttributePolyfill Attribute => new("System", "Attribute", AttributeCode);

	private const string AttributeTargetsCode = """
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
	public static AttributePolyfill AttributeTargets => new("System", "AttributeTargets", AttributeTargetsCode);

	private const string AttributeUsageAttributeCode = """
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
	public static AttributePolyfill AttributeUsageAttribute => new("System", "AttributeUsageAttribute", AttributeUsageAttributeCode);

	private const string AssemblyVersionAttributeCode = """
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
	public static AttributePolyfill AssemblyVersionAttribute => new("System.Reflection", "AssemblyVersionAttribute", AssemblyVersionAttributeCode);

	private const string MethodImplAttributesCode = """
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
	public static AttributePolyfill MethodImplAttributes => new("System.Reflection", "MethodImplAttributes", MethodImplAttributesCode);

	private const string MethodCodeTypeCode = """
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
	public static AttributePolyfill MethodCodeType => new("System.Runtime.CompilerServices", "MethodCodeType", MethodCodeTypeCode);

	private const string MethodImplOptionsCode = """
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
	public static AttributePolyfill MethodImplOptions => new("System.Runtime.CompilerServices", "MethodImplOptions", MethodImplOptionsCode);

	private const string MethodImplAttributeCode = """
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
	public static AttributePolyfill MethodImplAttribute => new("System.Runtime.CompilerServices", "MethodImplAttribute", MethodImplAttributeCode);

	private const string TypeForwardedToAttributeCode = """
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
	public static AttributePolyfill TypeForwardedToAttribute => new("System.Runtime.CompilerServices", "TypeForwardedToAttribute", TypeForwardedToAttributeCode);

	/// <summary>
	/// Modified version of the IsUnmanagedAttribute from the .NET runtime.
	/// References to System.ComponentModel have been removed because they're in System.dll instead of mscorlib.
	/// </summary>
	private const string IsUnmanagedAttributeCode = """
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
	public static AttributePolyfill IsUnmanagedAttribute => new("System.Runtime.CompilerServices", "IsUnmanagedAttribute", IsUnmanagedAttributeCode);

	public static IEnumerable<AttributePolyfill> GetPolyfills() =>
	[
		Attribute,
		AttributeTargets,
		AttributeUsageAttribute,
		AssemblyVersionAttribute,
		MethodImplAttributes,
		MethodCodeType,
		MethodImplOptions,
		MethodImplAttribute,
		TypeForwardedToAttribute,
		IsUnmanagedAttribute,
	];
}
