using System;
using System.Collections.Generic;
using System.Linq;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class SceneExportCollection : IExportCollection, IComparer<Object>
	{
		public SceneExportCollection(IAssetExporter assetExporter, string name, IEnumerable<Object> objects)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			AssetExporter = assetExporter;
			Name = name;

			HashSet<string> exportIDs = new HashSet<string>();
			foreach (Object @object in objects.OrderBy(t => t, this))
			{
				AddObject(@object, exportIDs);
			}

			if (Config.IsGenerateGUIDByContent)
			{
				GUID = ObjectUtils.CalculateObjectsGUID(objects);
			}
			else
			{
				GUID = new UtinyGUID(new Guid());
			}
		}

		public bool IsContains(Object @object)
		{
			return m_exportIDs.ContainsKey(@object);
		}

		public string GetExportID(Object @object)
		{
			return m_exportIDs[@object];
		}

		public ExportPointer CreateExportPointer(Object @object, bool isLocal)
		{
			string exportID = GetExportID(@object);
			return isLocal ?
				new ExportPointer(exportID) :
				new ExportPointer(exportID, GUID, AssetType.Serialized);
		}

		public int Compare(Object obj1, Object obj2)
		{
			if(IsSceneObject(obj1))
			{
				if(IsSceneObject(obj2))
				{
					if(obj1.ClassID == obj2.ClassID)
					{
						return 0;
					}
					else
					{
						return obj1.ClassID < obj2.ClassID ? 1 : -1;
					}
				}
				return -1;
			}
			if(IsSceneObject(obj2))
			{
				return 1;
			}
			return 0;
		}

		private void AddObject(Object @object, HashSet<string> exportIDs)
		{
			string exportID;
			switch(@object.ClassID)
			{
				case ClassIDType.SceneSettings:
					exportID = 1.ToString();
					break;

				case ClassIDType.RenderSettings:
					exportID = 2.ToString();
					break;

				case ClassIDType.LightmapSettings:
					exportID = 3.ToString();
					break;

				case ClassIDType.NavMeshSettings:
					exportID = 4.ToString();
					break;

				default:
					exportID = ObjectUtils.GenerateExportID(@object, (t) => exportIDs.Contains(t));
					break;
			}

			m_exportIDs.Add(@object, exportID);
			exportIDs.Add(exportID);
		}

		private static bool IsSceneObject(Object obj1)
		{
			switch (obj1.ClassID)
			{
				case ClassIDType.SceneSettings:
				case ClassIDType.RenderSettings:
				case ClassIDType.LightmapSettings:
				case ClassIDType.NavMeshSettings:
					return true;

				default:
					return false;
			}
		}

		public IAssetExporter AssetExporter { get; }
		public IEnumerable<Object> Objects => m_exportIDs.Keys;
		public string Name { get; }
		public UtinyGUID GUID { get; }
		public IYAMLExportable MetaImporter { get; } = new DefaultImporter(false);

		private readonly Dictionary<Object, string> m_exportIDs = new Dictionary<Object, string>();
	}
}
