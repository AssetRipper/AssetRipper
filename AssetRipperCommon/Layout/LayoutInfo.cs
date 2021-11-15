using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Layout
{
	/// <summary>
	/// A class for holding the Version, Platform, and Transfer Instruction Flags
	/// </summary>
	public sealed class LayoutInfo
	{
		static LayoutInfo()
		{
			ClassNames = InitializeClassNames();
		}

		/// <summary>
		/// 2.1.0 and greater
		/// The alignment concept was first introduced only in v2.1.0
		/// </summary>
		public bool IsAlign { get; }
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public bool IsAlignArrays { get; }
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public bool IsStructSerializable { get; }

		public UnityVersion Version { get; }
		public Platform Platform { get; }
		public TransferInstructionFlags Flags { get; }

		public LayoutInfo(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
			IsAlign = Version.IsGreaterEqual(2, 1);
			IsAlignArrays = Version.IsGreaterEqual(2017);
			IsStructSerializable = Version.IsGreaterEqual(4, 5);
		}

		public static bool operator ==(LayoutInfo lhs, LayoutInfo rhs)
		{
			if (lhs.Version != rhs.Version)
			{
				return false;
			}
			if (lhs.Platform != rhs.Platform)
			{
				return false;
			}
			if (lhs.Flags != rhs.Flags)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(LayoutInfo lhs, LayoutInfo rhs)
		{
			if (lhs.Version != rhs.Version)
			{
				return true;
			}
			if (lhs.Platform != rhs.Platform)
			{
				return true;
			}
			if (lhs.Flags != rhs.Flags)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 647;
			unchecked
			{
				hash = hash + 1063 * Version.GetHashCode();
				hash = hash * 347 + Platform.GetHashCode();
				hash = hash * 557 + Flags.GetHashCode();
			}
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj is LayoutInfo info)
			{
				return info == this;
			}
			return false;
		}

		public override string ToString()
		{
			return $"v{Version} {Platform} [{Flags}]";
		}

		private static Dictionary<ClassIDType, string> InitializeClassNames()
		{
			Dictionary<ClassIDType, string> names = new Dictionary<ClassIDType, string>();
			ClassIDType[] classTypes = (ClassIDType[])System.Enum.GetValues(typeof(ClassIDType));
			foreach (ClassIDType classType in classTypes)
			{
				names[classType] = classType.ToString();
			}
			return names;
		}

		public string GetClassName(ClassIDType classID)
		{
			if (classID == ClassIDType.PrefabInstance)
				return GetPrefabClassName(Version);
			else if (ClassNames.TryGetValue(classID, out string name))
				return name;
			else
				return null;
		}


		private static IReadOnlyDictionary<ClassIDType, string> ClassNames { get; }

		private static string GetPrefabClassName(UnityVersion version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return nameof(ClassIDType.PrefabInstance);
			}
			else if (version.IsGreaterEqual(3, 5))
			{
				return nameof(ClassIDType.Prefab);
			}
			else
			{
				return nameof(ClassIDType.DataTemplate);
			}
		}


		public const string TypelessdataName = "_typelessdata";
	}
}
