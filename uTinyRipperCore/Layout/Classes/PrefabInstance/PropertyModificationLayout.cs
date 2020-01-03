using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class PropertyModificationLayout
	{
		public PropertyModificationLayout(LayoutInfo info)
		{
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			PropertyModificationLayout layout = context.Layout.PrefabInstance.PropertyModification;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddPPtr(context.Layout.Object.Name, layout.TargetName);
			context.AddString(layout.PropertyPathName);
			context.AddString(layout.ValueName);
			context.AddPPtr(context.Layout.Object.Name, layout.ObjectReferenceName);
			context.EndChildren();
		}

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
