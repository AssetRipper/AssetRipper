using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public sealed class EmptyExportCollection : ExportCollectionBase
{
	public static EmptyExportCollection Instance { get; } = new();
	public override bool Contains(IUnityObjectBase asset) => false;
	public override bool Export(string projectDirectory, FileSystem fileSystem) => true;
	public override IContentExtractor ContentExtractor => throw new NotSupportedException();
	public override IEnumerable<IUnityObjectBase> Assets => [];
	public override string Name => nameof(EmptyExportCollection);
	public override bool Exportable => false;
}
