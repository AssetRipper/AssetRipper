using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Project;
using System;
using System.Linq;
using GObject = AssetRipper.Core.Classes.GameObject.GameObject;

namespace AssetRipper.Core.Converters.GameObject
{
	public static class GameObjectConverter
	{
		public static GObject Convert(IExportContainer container, GObject origin)
		{
			GObject instance = new GObject(container.ExportLayout);
			EditorExtensionConverter.Convert(container, origin, instance);
			if (GObject.IsComponentTuple(container.ExportVersion))
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
			if (GObject.HasTag(container.ExportVersion, container.ExportFlags))
			{
				instance.Tag = GetTag(container, origin);
			}
			if (GObject.HasTagString(container.ExportVersion, container.ExportFlags))
			{
				instance.TagString = GetTagString(container, origin);
			}
			return instance;
		}

		private static ComponentPair[] GetComponent(IExportContainer container, GObject origin)
		{
			if (GObject.IsComponentTuple(container.Version))
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

		private static bool GetIsActive(IExportContainer container, GObject origin)
		{
			if (GObject.IsActiveInherited(container.Version))
			{
				return origin.SerializedFile.Collection.IsScene(origin.SerializedFile) ? origin.IsActive : true;
			}
			return origin.IsActive;
		}

		private static ushort GetTag(IExportContainer container, GObject origin)
		{
			if (GObject.HasTag(container.Version, container.Flags))
			{
				return origin.Tag;
			}
			return container.TagNameToID(origin.TagString);
		}

		private static string GetTagString(IExportContainer container, GObject origin)
		{
			if (GObject.HasTagString(container.Version, container.Flags))
			{
				return origin.TagString;
			}
			return container.TagIDToName(origin.Tag);
		}
	}
}
