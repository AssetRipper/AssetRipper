using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets.Export.Dependencies
{
	public interface IDependent
	{
		IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context);
		IEnumerable<(FieldName, PPtr<IUnityObjectBase>)> FetchDependencies(FieldName? parent);
	}
}
