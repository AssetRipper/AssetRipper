using System;
using System.Collections.Generic;

namespace uTinyRipper.Layout
{
	public sealed class AssetLayout
	{
		public AssetLayout(LayoutInfo info)
		{
			Info = info;

			IsAlign = info.Version.IsGreaterEqual(2, 1);
			IsAlignArrays = info.Version.IsGreaterEqual(2017);
			IsStructSerializable = info.Version.IsGreaterEqual(4, 5);

			PPtr = new PPtrLayout(info);

			Misc = new MiscLayoutCategory(info);
			Serialized = new SerializedLayoutCategory(info);

			Animation = new AnimationLayout(info);
			AnimationClip = new AnimationClipLayout(info);
			Behaviour = new BehaviourLayout(info);
			Component = new ComponentLayout(info);
			EditorExtension = new EditorExtensionLayout(info);
			Font = new FontLayout(info);
			GameObject = new GameObjectLayout(info);
			MonoBehaviour = new MonoBehaviourLayout(info);
			MonoScript = new MonoScriptLayout(info);
			NamedObject = new NamedObjectLayout(info);
			Object = new ObjectLayout(info);
			Prefab = new PrefabLayout(info);
			PrefabInstance = new PrefabInstanceLayout(info);
			Texture2D = new Texture2DLayout(info);
			Transform = new TransformLayout(info);

			ClassNames = CreateClassNames();
		}

		private Dictionary<ClassIDType, string> CreateClassNames()
		{
			Dictionary<ClassIDType, string> names = new Dictionary<ClassIDType, string>();
			ClassIDType[] classTypes = (ClassIDType[])Enum.GetValues(typeof(ClassIDType));
			foreach (ClassIDType classType in classTypes)
			{
				names[classType] = classType.ToString();
			}
			names[ClassIDType.PrefabInstance] = PrefabInstance.Name;
			return names;
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

		public IReadOnlyDictionary<ClassIDType, string> ClassNames { get; }

		public PPtrLayout PPtr { get; }

		public MiscLayoutCategory Misc { get; }
		public SerializedLayoutCategory Serialized { get; }

		public AnimationLayout Animation { get; }
		public AnimationClipLayout AnimationClip { get; }
		public BehaviourLayout Behaviour { get; }
		public ComponentLayout Component { get; }
		public EditorExtensionLayout EditorExtension { get; }
		public FontLayout Font { get; }
		public GameObjectLayout GameObject { get; }
		public MonoBehaviourLayout MonoBehaviour { get; }
		public MonoScriptLayout MonoScript { get; }
		public NamedObjectLayout NamedObject { get; }
		public ObjectLayout Object { get; }
		public PrefabLayout Prefab { get; }
		public PrefabInstanceLayout PrefabInstance { get; }
		public Texture2DLayout Texture2D { get; }
		public TransformLayout Transform { get; }

		public string TypelessdataName => "_typelessdata";
	}
}
