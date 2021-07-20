using AssetRipper.Converters.Project;
using AssetRipper.Layout.Classes.GameObject;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes;
using AssetRipper.Parser.Classes.GameObject;
using AssetRipper.Parser.Classes.Misc;
using System;
using System.Linq;

namespace AssetRipper.Converters.Classes.GameObject
{
	public static class GameObjectConverter
	{
		public static Parser.Classes.GameObject.GameObject Convert(IExportContainer container, Parser.Classes.GameObject.GameObject origin)
		{
			GameObjectLayout layout = container.Layout.GameObject;
			GameObjectLayout exlayout = container.ExportLayout.GameObject;
			Parser.Classes.GameObject.GameObject instance = new Parser.Classes.GameObject.GameObject(container.ExportLayout);
			EditorExtensionConverter.Convert(container, origin, instance);
			if (exlayout.IsComponentTuple)
			{
				instance.ComponentTuple = origin.ComponentTuple.ToArray();
			}
			else
			{
				instance.Component = GetComponent(container, origin);
			}
			instance.IsActive = GetIsActive(container, origin);
			instance.Layer = origin.Layer;
			instance.Name = origin.Name;
			if (exlayout.HasTag)
			{
				instance.Tag = GetTag(container, origin);
			}
			if (exlayout.HasTagString)
			{
				instance.TagString = GetTagString(container, origin);
			}
#if UNIVERSAL
			if (layout.HasIcon)
			{
				instance.Icon = origin.Icon;
			}
			if (layout.HasNavMeshLayer)
			{
				instance.NavMeshLayer = origin.NavMeshLayer;
				instance.StaticEditorFlags = origin.StaticEditorFlags;
			}
			else if (exlayout.HasIsStatic && layout.HasIsStatic)
			{
				instance.IsStatic = origin.IsStatic;
			}
#endif
			return instance;
		}

		private static ComponentPair[] GetComponent(IExportContainer container, Parser.Classes.GameObject.GameObject origin)
		{
			if (container.Layout.GameObject.IsComponentTuple)
			{
				Tuple<ClassIDType, PPtr<Component>>[] originComponent = origin.ComponentTuple;
				ComponentPair[] pairs = new ComponentPair[originComponent.Length];
				for (int i = 0; i < pairs.Length; i++)
				{
					ComponentPair pair = new ComponentPair();
					pair.Component = originComponent[i].Item2;
					pairs[i] = pair;
				}
				return pairs;
			}
			else
			{
				return origin.Component.Select(t => ComponentPairConverter.Convert(container, t)).ToArray();
			}
		}

		private static bool GetIsActive(IExportContainer container, Parser.Classes.GameObject.GameObject origin)
		{
			if (container.Layout.GameObject.IsActiveInherited)
			{
				return origin.File.Collection.IsScene(origin.File) ? origin.IsActive : true;
			}
			return origin.IsActive;
		}

		private static ushort GetTag(IExportContainer container, Parser.Classes.GameObject.GameObject origin)
		{
			if (container.Layout.GameObject.HasTag)
			{
				return origin.Tag;
			}
			return container.TagNameToID(origin.TagString);
		}

		private static string GetTagString(IExportContainer container, Parser.Classes.GameObject.GameObject origin)
		{
			if (container.Layout.GameObject.HasTagString)
			{
				return origin.TagString;
			}
			return container.TagIDToName(origin.Tag);
		}
	}
}
