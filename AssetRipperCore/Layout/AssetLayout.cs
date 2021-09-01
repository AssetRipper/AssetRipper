using AssetRipper.Core.Layout.Categories;
using AssetRipper.Core.Layout.Classes;
using AssetRipper.Core.Layout.Classes.AnimationClip;
using AssetRipper.Core.Layout.Classes.GameObject;
using AssetRipper.Core.Layout.Classes.PrefabInstance;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Layout
{
	public sealed class AssetLayout
	{
		static AssetLayout()
		{
			ClassNames = InitializeClassNames();
		}

		public AssetLayout(LayoutInfo info)
		{
			Info = info;

			IsAlign = info.Version.IsGreaterEqual(2, 1);
			IsAlignArrays = info.Version.IsGreaterEqual(2017);
			IsStructSerializable = info.Version.IsGreaterEqual(4, 5);

			Misc = new MiscLayoutCategory(info);
			Serialized = new SerializedLayoutCategory(info);

			Animation = new AnimationLayout(info);
			AnimationClip = new AnimationClipLayout(info);
			Behaviour = new BehaviourLayout(info);
			Component = new ComponentLayout(info);
			EditorExtension = new EditorExtensionLayout(info);
			GameObject = new GameObjectLayout(info);
			MonoBehaviour = new MonoBehaviourLayout(info);
			Object = new ObjectLayout(info);
			Prefab = new PrefabLayout(info);
			PrefabInstance = new PrefabInstanceLayout(info);
		}

		private static Dictionary<ClassIDType, string> InitializeClassNames()
		{
			Dictionary<ClassIDType, string> names = new Dictionary<ClassIDType, string>();
			ClassIDType[] classTypes = (ClassIDType[])Enum.GetValues(typeof(ClassIDType));
			foreach (ClassIDType classType in classTypes)
			{
				names[classType] = classType.ToString();
			}
			return names;
		}

		public string GetClassName(ClassIDType classID)
		{
			if (classID == ClassIDType.PrefabInstance)
				return PrefabInstance.Name;
			else if (ClassNames.TryGetValue(classID, out string name))
				return name;
			else
				return null;
		}

		public LayoutInfo Info { get; }

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

		private static IReadOnlyDictionary<ClassIDType, string> ClassNames { get; }

		public MiscLayoutCategory Misc { get; }
		public SerializedLayoutCategory Serialized { get; }

		public AnimationLayout Animation { get; }
		public AnimationClipLayout AnimationClip { get; }
		public BehaviourLayout Behaviour { get; }
		public ComponentLayout Component { get; }
		public EditorExtensionLayout EditorExtension { get; }
		public GameObjectLayout GameObject { get; }
		public MonoBehaviourLayout MonoBehaviour { get; }
		public ObjectLayout Object { get; }
		public PrefabLayout Prefab { get; }
		public PrefabInstanceLayout PrefabInstance { get; }

		public string TypelessdataName => "_typelessdata";
	}
}
