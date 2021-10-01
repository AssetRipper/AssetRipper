
namespace AssetRipper.Core.Layout.Classes.PrefabInstance
{
	public sealed class PropertyModificationLayout
	{
		public PropertyModificationLayout(LayoutInfo info) { }

		public int Version = 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasTarget => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasPropertyPath => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasValue => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasObjectReference => true;

		public string Name { get; }
		public string TargetName => "target";
		public string PropertyPathName => "propertyPath";
		public string ValueName => "value";
		public string ObjectReferenceName => "objectReference";
	}
}
