using AssetRipper.AssemblyDumper.Passes;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper;

public static class Program
{
	public static void Main(string[] args)
	{
		RunGeneration();
	}

	private static void RunGeneration()
	{
		using (new TimingCookie("Pass 000: Initialization"))
		{
			Pass000_ProcessTpk.IntitializeSharedState("type_tree.tpk");
		}
		using (new TimingCookie("Pass 001: Merge Moved Groups"))
		{
			Pass001_MergeMovedGroups.DoPass();
		}
		using (new TimingCookie("Pass 002: Rename Subnodes"))
		{
			Pass002_RenameSubnodes.DoPass();
		}
		using (new TimingCookie("Pass 003: Fix TextureImporter Nodes"))
		{
			Pass003_FixTextureImporterNodes.DoPass();
		}
		using (new TimingCookie("Pass 004: Fill Name to Type Id Dictionary"))
		{
			Pass004_FillNameToTypeIdDictionary.DoPass();
		}
		using (new TimingCookie("Pass 005: Split Abstract Classes"))
		{
			Pass005_SplitAbstractClasses.DoPass();
		}
		using (new TimingCookie("Pass 007: Extract Subclasses"))
		{
			Pass007_ExtractSubclasses.DoPass();
		}
		using (new TimingCookie("Pass 008: Divide Ambiguous PPtr"))
		{
			Pass008_DivideAmbiguousPPtr.DoPass();
		}
		using (new TimingCookie("Pass 009: Create Groups"))
		{
			Pass009_CreateGroups.DoPass();
		}
		using (new TimingCookie("Pass 010: Initialize Interfaces"))
		{
			Pass010_InitializeInterfacesAndFactories.DoPass();
		}
		using (new TimingCookie("Pass 011: Apply Inheritance"))
		{
			Pass011_ApplyInheritance.DoPass();
		}
		using (new TimingCookie("Pass 012: Apply Correct Type Attributes"))
		{
			Pass012_ApplyCorrectTypeAttributes.DoPass();
		}
		using (new TimingCookie("Pass 013: Unify Fields of Abstract Types"))
		{
			Pass013_UnifyFieldsOfAbstractTypes.DoPass();
		}
		using (new TimingCookie("Pass 015: Add Fields"))
		{
			Pass015_AddFields.DoPass();
		}
		using (new TimingCookie("Pass 039: Inject Enum Values"))
		{
			Pass039_InjectEnumValues.DoPass();
		}
		using (new TimingCookie("Pass 040: Add Enum Types"))
		{
			Pass040_AddEnums.DoPass();
		}
		using (new TimingCookie("Pass 041: Add Native Enum Types"))
		{
			Pass041_NativeEnums.DoPass();
		}
		using (new TimingCookie("Pass 045: Marker Interfaces"))
		{
			Pass045_AddMarkerInterfaces.DoPass();
		}
		using (new TimingCookie("Pass 052: Interface Properties and Methods"))
		{
			Pass052_InterfacePropertiesAndMethods.DoPass();
		}
		using (new TimingCookie("Pass 053: Has Methods and Nullable Attributes"))
		{
			Pass053_HasMethodsAndNullableAttributes.DoPass();
		}
		using (new TimingCookie("Pass 054: Assign Property Histories"))
		{
			Pass054_AssignPropertyHistories.DoPass();
		}
		using (new TimingCookie("Pass 055: Create Enum Properties"))
		{
			Pass055_CreateEnumProperties.DoPass();
		}
		using (new TimingCookie("Pass 058: Inject Chinese Texture Properties"))
		{
			Pass058_InjectChineseTextureProperties.DoPass();
		}
		using (new TimingCookie("Pass 061: Add Constructors"))
		{
			Pass061_AddConstructors.DoPass();
		}
		using (new TimingCookie("Pass 062: Fill Constructors"))
		{
			Pass062_FillConstructors.DoPass();
		}
		using (new TimingCookie("Pass 063: Create Empty Methods"))
		{
			Pass063_CreateEmptyMethods.DoPass();
		}
		using (new TimingCookie("Pass 080: PPtr Conversions"))
		{
			Pass080_PPtrConversions.DoPass();
		}
		using (new TimingCookie("Pass 081: PPtr Properties"))
		{
			Pass081_CreatePPtrProperties.DoPass();
		}
		using (new TimingCookie("Pass 100: Filling Read Methods"))
		{
			Pass100_FillReadMethods.DoPass();
		}
		using (new TimingCookie("Pass 101: Filling Write Methods"))
		{
			Pass101_FillWriteMethods.DoPass();
		}
		using (new TimingCookie("Pass 102: Ignore Field In Meta Files Methods"))
		{
			Pass102_IgnoreFieldInMetaFilesMethods.DoPass();
		}
		using (new TimingCookie("Pass 103: Filling Dependency Methods"))
		{
			Pass103_FillDependencyMethods.DoPass();
		}
		using (new TimingCookie("Pass 104: Reset Methods"))
		{
			Pass104_ResetMethods.DoPass();
		}
		using (new TimingCookie("Pass 105: CopyValues Methods"))
		{
			Pass105_CopyValuesMethods.DoPass();
		}
		using (new TimingCookie("Pass 108: Walk Methods"))
		{
			Pass108_WalkMethods.DoPass();
		}
		using (new TimingCookie("Pass 110: Class Name and ID Overrides"))
		{
			Pass110_ClassNameAndIdOverrides.DoPass();
		}
		using (new TimingCookie("Pass 201: GUID Explicit Conversion"))
		{
			Pass201_GuidConversionOperators.DoPass();
		}
		using (new TimingCookie("Pass 202: Vector Explicit Conversions"))
		{
			Pass202_VectorExplicitConversions.DoPass();
		}
		using (new TimingCookie("Pass 203: OffsetPtr Implicit Conversions"))
		{
			Pass203_OffsetPtrImplicitConversions.DoPass();
		}
		using (new TimingCookie("Pass 204: Hash128 Explicit Conversion"))
		{
			Pass204_Hash128ExplicitConversion.DoPass();
		}
		using (new TimingCookie("Pass 205: Color Explicit Conversions"))
		{
			Pass205_ColorExplicitConversions.DoPass();
		}
		using (new TimingCookie("Pass 206: BoneWeights4 Explicit Conversions"))
		{
			Pass206_BoneWeights4ExplicitConversions.DoPass();
		}
		using (new TimingCookie("Pass 300: Named Interface"))
		{
			Pass300_NamedInterface.DoPass();
		}
		using (new TimingCookie("Pass 301: SourcePrefab Property"))
		{
			Pass301_SourcePrefabProperty.DoPass();
		}
		using (new TimingCookie("Pass 400: Equality Comparison"))
		{
			Pass400_EqualityComparison.DoPass();
		}
		using (new TimingCookie("Pass 410: SetValues Methods"))
		{
			Pass410_SetValuesMethods.DoPass();
		}
		using (new TimingCookie("Pass 500: Fixing PPtr Yaml"))
		{
			Pass500_PPtrFixes.DoPass();
		}
		using (new TimingCookie("Pass 501: Fixing MonoBehaviour"))
		{
			Pass501_MonoBehaviourImplementation.DoPass();
		}
		using (new TimingCookie("Pass 502: Fixing Guid and Hash Yaml Export"))
		{
			Pass502_FixGuidAndHashYaml.DoPass();
		}
		using (new TimingCookie("Pass 504: Fixing Shader Name"))
		{
			Pass504_FixShaderName.DoPass();
		}
		using (new TimingCookie("Pass 505: Fixing Old AudioClips"))
		{
			Pass505_FixOldAudioClip.DoPass();
		}
		using (new TimingCookie("Pass 506: Fixing UnityConnectSettings"))
		{
			Pass506_FixUnityConnectSettings.DoPass();
		}
		using (new TimingCookie("Pass 507: Inject Properties"))
		{
			Pass507_InjectedProperties.DoPass();
		}
		using (new TimingCookie("Pass 508: Lazy SceneObjectIdentifier"))
		{
			Pass508_LazySceneObjectIdentifier.DoPass();
		}
		using (new TimingCookie("Pass 510: Fix Component Pair Walking"))
		{
			Pass510_FixComponentPairWalking.DoPass();
		}
		using (new TimingCookie("Pass 555: Create Common String"))
		{
			Pass555_CreateCommonString.DoPass();
		}
		using (new TimingCookie("Pass 556: Create ClassIDType Enum"))
		{
			Pass556_CreateClassIDTypeEnum.DoPass();
		}
		using (new TimingCookie("Pass 557: Create SourceTpk Class"))
		{
			Pass557_CreateSourceTpkClass.DoPass();
		}
		using (new TimingCookie("Pass 558: Create Type to ClassIDType Dictionary"))
		{
			Pass558_TypeCache.DoPass();
		}
		using (new TimingCookie("Pass 920: Interface Inheritance"))
		{
			Pass920_InterfaceInheritance.DoPass();
		}
		using (new TimingCookie("Pass 940: Make Asset Factory"))
		{
			Pass940_MakeAssetFactory.DoPass();
		}
		using (new TimingCookie("Pass 941: Make Field Hashes"))
		{
			Pass941_MakeFieldHashes.DoPass();
		}
		using (new TimingCookie("Pass 998: Write Assembly"))
		{
			Pass998_SaveAssembly.DoPass();
		}
		using (new TimingCookie("Pass 999: Generate Documentation"))
		{
			Pass999_Documentation.DoPass();
		}
	}

	private readonly struct TimingCookie : IDisposable
	{
		private readonly Stopwatch stopWatch = new();

		public TimingCookie(string message)
		{
			Console.WriteLine(message);
			stopWatch.Start();
		}

		public void Dispose()
		{
			stopWatch.Stop();
			Console.WriteLine($"\tFinished in {stopWatch.ElapsedMilliseconds} ms");
		}
	}
}
