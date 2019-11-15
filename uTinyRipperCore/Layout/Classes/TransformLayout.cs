using uTinyRipper.Classes;

namespace uTinyRipper.Layout
{
	public sealed class TransformLayout
	{
		public TransformLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(4, 5) && !info.Flags.IsRelease())
			{
				HasRootOrder = true;
			}
			if (info.Version.IsGreaterEqual(5) && !info.Flags.IsRelease())
			{
				HasLocalEulerAnglesHint = true;
			}
		}

		public int Version => 1;

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasLocalRotation => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasLocalPosition => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasLocalScale => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasChildren => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasFather => true;
		/// <summary>
		/// 4.5.0 and greater and Not Release
		/// </summary>
		public bool HasRootOrder { get; }
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public bool HasLocalEulerAnglesHint { get; }

		public string Name => nameof(Transform);
		public string LocalRotationName => "m_LocalRotation";
		public string LocalPositionName => "m_LocalPosition";
		public string LocalScaleName => "m_LocalScale";
		public string ChildrenName => "m_Children";
		public string FatherName => "m_Father";
		public string RootOrderName => "m_RootOrder";
		public string LocalEulerAnglesHintName => "m_LocalEulerAnglesHint";
	}
}
