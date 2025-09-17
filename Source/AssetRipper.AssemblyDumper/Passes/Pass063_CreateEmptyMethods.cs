using AssetRipper.Assets;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass063_CreateEmptyMethods
{
	public const MethodAttributes OverrideMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot | MethodAttributes.HideBySig;

	public static void DoPass()
	{
		TypeSignature commonPPtrTypeRef = SharedState.Instance.Importer.ImportTypeSignature(typeof(PPtr<>));
		TypeSignature unityObjectBaseInterfaceRef = SharedState.Instance.Importer.ImportTypeSignature<IUnityObjectBase>();
		GenericInstanceTypeSignature unityObjectBasePPtrRef = commonPPtrTypeRef.MakeGenericInstanceType(unityObjectBaseInterfaceRef);
		TypeSignature refEndianSpanReaderRef = SharedState.Instance.Importer.ImportTypeSignature(typeof(EndianSpanReader)).MakeByReferenceType();
		TypeSignature assetWriterRef = SharedState.Instance.Importer.ImportTypeSignature<AssetWriter>();

		foreach (TypeDefinition type in SharedState.Instance.AllNonInterfaceTypes)
		{
			type.AddMethod(nameof(UnityAssetBase.ReadRelease), OverrideMethodAttributes, SharedState.Instance.Importer.Void)
				.AddParameter(refEndianSpanReaderRef, "reader");

			type.AddMethod(nameof(UnityAssetBase.ReadEditor), OverrideMethodAttributes, SharedState.Instance.Importer.Void)
				.AddParameter(refEndianSpanReaderRef, "reader");

			type.AddMethod(nameof(UnityAssetBase.WriteRelease), OverrideMethodAttributes, SharedState.Instance.Importer.Void)
				.AddParameter(assetWriterRef, "writer");

			type.AddMethod(nameof(UnityAssetBase.WriteEditor), OverrideMethodAttributes, SharedState.Instance.Importer.Void)
				.AddParameter(assetWriterRef, "writer");

			type.AddMethod(nameof(UnityAssetBase.Reset), OverrideMethodAttributes, SharedState.Instance.Importer.Void);
		}
	}
}