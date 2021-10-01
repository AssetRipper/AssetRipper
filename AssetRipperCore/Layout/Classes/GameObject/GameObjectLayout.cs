using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Layout.Classes.GameObject
{
	public sealed class GameObjectLayout
	{
		public GameObjectLayout(LayoutInfo info)
		{
			ComponentPair = new ComponentPairLayout(info);

			if (info.Version.IsGreaterEqual(5, 5))
			{
				// unknown
				Version = 5;
			}
			else if (info.Version.IsGreaterEqual(4))
			{
				// active state inheritance
				Version = 4;
			}
			else
			{
				// min is 3
				// tag is ushort for Release, otherwise string. For later versions for yaml only string left
				Version = 3;

				// tag is string
				// Version = 2;
				// tag is ushort
				// Version = 1;
			}

			if (info.Flags.IsRelease() || info.Version.IsLess(2, 1))
			{
				HasTag = true;
			}
			if (info.Version.IsGreaterEqual(2, 1) && !info.Flags.IsRelease())
			{
				HasTagString = true;
			}
			if (info.Version.IsGreaterEqual(3, 5) && !info.Flags.IsRelease())
			{
				HasNavMeshLayer = true;
				HasStaticEditorFlags = true;
			}
			if (info.Version.IsGreaterEqual(3) && info.Version.IsLess(3, 5) && !info.Flags.IsRelease())
			{
				HasIsStatic = true;
			}
			if (info.Version.IsGreaterEqual(3, 4) && !info.Flags.IsRelease())
			{
				HasIcon = true;
			}

			if (info.Version.IsLess(5, 5))
			{
				IsComponentTuple = true;
			}
			if (info.Version.IsLess(2, 1))
			{
				IsActiveFirst = true;
			}
			if (info.Version.IsGreaterEqual(2, 1))
			{
				IsNameFirst = true;
			}
			if (info.Version.IsGreaterEqual(3, 5))
			{
				IsIconFirst = true;
			}

			if (info.Version.IsLess(4))
			{
				IsActiveInherited = true;
			}
		}

		public ComponentPairLayout ComponentPair { get; }

		public int Version { get; }

		/// <summary>
		/// Release or less than 2.1.0
		/// </summary>
		public bool HasTag { get; }
		/// <summary>
		/// 2.1.0 and greater and Not Release
		/// </summary>
		public bool HasTagString { get; }
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public bool HasNavMeshLayer { get; }
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public bool HasStaticEditorFlags { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasIsActive => true;
		/// <summary>
		/// 3.0.0 to 3.5.0 exclusive and Not Release
		/// </summary>
		public bool HasIsStatic { get; }
		/// <summary>
		/// 3.4.0 and Not Release
		/// </summary>
		public bool HasIcon { get; }

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public bool IsComponentTuple { get; }
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		public bool IsActiveFirst { get; }
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public bool IsNameFirst { get; }
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public bool IsIconFirst { get; }

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public bool IsActiveInherited { get; }

		public string Name => nameof(AssetRipper.Core.Classes.GameObject.GameObject);
		public string ComponentName => "m_Component";
		public string LayerName => "m_Layer";
		public string NameName => "m_Name";
		public string TagName => "m_Tag";
		public string TagStringName => "m_TagString";
		public string NavMeshLayerName => "m_NavMeshLayer";
		public string StaticEditorFlagsName => "m_StaticEditorFlags";
		public string IsActiveName => "m_IsActive";
		public string IsStaticName => "m_IsStatic";
		public string IconName => "m_Icon";
	}
}
