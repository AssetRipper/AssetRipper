using AssemblyDumper.Utils;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Diagnostics.CodeAnalysis;

namespace AssemblyDumper
{
	public static class CommonTypeGetter
	{
		public static AssemblyDefinition? CommonAssembly { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		//Reading
		public static ITypeDefOrRef AssetReaderDefinition { get; private set; }
		public static ITypeDefOrRef EndianReaderDefinition { get; private set; }
		public static ITypeDefOrRef EndianReaderExtensionsDefinition { get; private set; }
		public static ITypeDefOrRef AssetReaderExtensionsDefinition { get; private set; }

		//Writing
		public static ITypeDefOrRef AssetWriterDefinition { get; private set; }

		//Yaml Export
		public static ITypeDefOrRef IExportContainerDefinition { get; private set; }
		public static ITypeDefOrRef YAMLNodeDefinition { get; private set; }
		public static ITypeDefOrRef YAMLMappingNodeDefinition { get; private set; }
		public static ITypeDefOrRef YAMLSequenceNodeDefinition { get; private set; }
		public static ITypeDefOrRef YAMLScalarNodeDefinition { get; private set; }
		public static IMethodDefOrRef YAMLMappingNodeConstructor { get; private set; }
		public static IMethodDefOrRef YAMLSequenceNodeConstructor { get; private set; }

		//Generics
		public static ITypeDefOrRef AssetDictionaryType { get; private set; }
		public static ITypeDefOrRef NullableKeyValuePair { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static void Initialize()
		{
			AssetReaderDefinition = SharedState.Importer.ImportCommonType<AssetReader>();
			EndianReaderDefinition = SharedState.Importer.ImportCommonType<EndianReader>();
			EndianReaderExtensionsDefinition = SharedState.Importer.ImportCommonType(typeof(AssetRipper.Core.IO.Extensions.EndianReaderExtensions));
			AssetReaderExtensionsDefinition = SharedState.Importer.ImportCommonType(typeof(AssetRipper.Core.IO.Extensions.AssetReaderExtensions));

			//Writing
			AssetWriterDefinition = SharedState.Importer.ImportCommonType<AssetWriter>();

			//Yaml Export
			IExportContainerDefinition = SharedState.Importer.ImportCommonType<IExportContainer>();
			YAMLNodeDefinition = SharedState.Importer.ImportCommonType<YAMLNode>();
			YAMLMappingNodeDefinition = SharedState.Importer.ImportCommonType<YAMLMappingNode>();
			YAMLSequenceNodeDefinition = SharedState.Importer.ImportCommonType<YAMLSequenceNode>();
			YAMLScalarNodeDefinition = SharedState.Importer.ImportCommonType<YAMLScalarNode>();
			YAMLMappingNodeConstructor = SharedState.Importer.ImportCommonConstructor<YAMLMappingNode>();
			YAMLSequenceNodeConstructor = SharedState.Importer.ImportCommonConstructor<YAMLSequenceNode>(1);

			//Generics
			AssetDictionaryType = SharedState.Importer.ImportCommonType("AssetRipper.Core.IO.AssetDictionary`2");
			NullableKeyValuePair = SharedState.Importer.ImportCommonType("AssetRipper.Core.IO.NullableKeyValuePair`2");
		}


		private static readonly Dictionary<string, TypeDefinition?> typeLookupCache = new Dictionary<string, TypeDefinition?>();
		public static TypeDefinition? LookupCommonType(string typeFullName)
		{
			if(!typeLookupCache.TryGetValue(typeFullName, out TypeDefinition? type))
			{
				type = CommonAssembly!.ManifestModule!.TopLevelTypes.SingleOrDefault(t => t.GetTypeFullName() == typeFullName);
				typeLookupCache.Add(typeFullName, type);
			}
			return type;
		}
		public static TypeDefinition? LookupCommonType(Type type) => LookupCommonType(type.FullName!);
		public static TypeDefinition? LookupCommonType<T>() => LookupCommonType(typeof(T));

		public static MethodDefinition LookupCommonMethod(string typeFullName, Func<MethodDefinition, bool> filter) => LookupCommonType(typeFullName)!.Methods.Single(filter);
		public static MethodDefinition LookupCommonMethod(Type type, Func<MethodDefinition, bool> filter) => LookupCommonMethod(type.FullName!, filter);
		public static MethodDefinition LookupCommonMethod<T>(Func<MethodDefinition, bool> filter) => LookupCommonMethod(typeof(T), filter);


		private static readonly Dictionary<string, ITypeDefOrRef> importedTypeCache = new Dictionary<string, ITypeDefOrRef>();
		public static ITypeDefOrRef ImportCommonType(this ReferenceImporter importer, string typeFullName)
		{
			if(!importedTypeCache.TryGetValue(typeFullName, out ITypeDefOrRef? type))
			{
				type = importer.ImportType(LookupCommonType(typeFullName)!);
				importedTypeCache.Add(typeFullName, type);
			}
			return type;
		}
		public static ITypeDefOrRef ImportCommonType(this ReferenceImporter importer, System.Type type) => importer.ImportType(LookupCommonType(type)!);
		public static ITypeDefOrRef ImportCommonType<T>(this ReferenceImporter importer) => importer.ImportType(LookupCommonType<T>()!);

		public static IMethodDefOrRef ImportCommonMethod(this ReferenceImporter importer, string typeFullName, Func<MethodDefinition, bool> filter)
		{
			return importer.ImportMethod(LookupCommonMethod(typeFullName, filter));
		}
		public static IMethodDefOrRef ImportCommonMethod(this ReferenceImporter importer, System.Type type, Func<MethodDefinition, bool> filter)
		{
			return importer.ImportMethod(LookupCommonMethod(type, filter));
		}
		public static IMethodDefOrRef ImportCommonMethod<T>(this ReferenceImporter importer, Func<MethodDefinition, bool> filter)
		{
			return importer.ImportMethod(LookupCommonMethod<T>(filter));
		}

		/// <summary>
		/// Import the default constructor for this type
		/// </summary>
		public static IMethodDefOrRef ImportCommonConstructor<T>(this ReferenceImporter importer) => importer.ImportCommonConstructor(typeof(T).FullName!, 0);
		public static IMethodDefOrRef ImportCommonConstructor<T>(this ReferenceImporter importer, int numParameters) => importer.ImportCommonConstructor(typeof(T).FullName!, numParameters);
		public static IMethodDefOrRef ImportCommonConstructor(this ReferenceImporter importer, string typeFullName) => importer.ImportCommonConstructor(typeFullName, 0);
		public static IMethodDefOrRef ImportCommonConstructor(this ReferenceImporter importer, string typeFullName, int numParameters)
		{
			return importer.ImportMethod(LookupCommonType(typeFullName)!.GetConstructor(numParameters));
		}
	}
}