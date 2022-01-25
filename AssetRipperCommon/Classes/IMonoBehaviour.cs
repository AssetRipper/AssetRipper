using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public interface IMonoBehaviour : IComponent, IHasName
	{
		PPtr<IMonoScript> ScriptPtr { get; }
		SerializableStructure Structure { get; set; }
	}

	public static class MonoBehaviourExtensions
	{
		/// <summary>
		/// Whether this MonoBeh belongs to scene/prefab hierarchy or not<br/>
		/// TODO: find out why GameObject may have a value like PPtr(0, 894) even though such game object doesn't exist
		/// </summary>
		public static bool IsSceneObject(this IMonoBehaviour monoBehaviour) => !monoBehaviour.GameObjectPtr.IsNull;
		public static bool IsScriptableObject(this IMonoBehaviour monoBehaviour) => monoBehaviour.Name.Length > 0;

		/// <summary>
		/// Reads the structure with an AssetReader
		/// </summary>
		public static void ReadStructure(this IMonoBehaviour monoBehaviour, AssetReader reader)
		{
			if (!monoBehaviour.SerializedFile.Collection.AssemblyManager.IsSet)
			{
				return;
			}

			IMonoScript script = monoBehaviour.ScriptPtr.FindAsset(monoBehaviour.SerializedFile);
			if (script == null)
			{
				return;
			}

			SerializableType behaviourType = script.GetBehaviourType();
			if (behaviourType == null)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, $"Unable to read {monoBehaviour.GetNameNotEmpty()}, because valid definition for script {script.GetNameNotEmpty()} wasn't found");
				return;
			}

			monoBehaviour.Structure = behaviourType.CreateSerializableStructure();
			try
			{
				monoBehaviour.Structure.Read(reader);
			}
			catch (System.Exception ex)
			{
				monoBehaviour.Structure = null;
				Logger.Log(LogType.Error, LogCategory.Import, $"Unable to read {monoBehaviour.GetNameNotEmpty()}, because script layout {script.GetNameNotEmpty()} mismatch binary content");
				Logger.Log(LogType.Debug, LogCategory.Import, $"Stack trace: {ex}");
			}
			return;
		}

		public static void MaybeWriteStructure(this IMonoBehaviour monoBehaviour, AssetWriter writer)
		{
			if (monoBehaviour.Structure != null)
			{
				monoBehaviour.Structure.Write(writer);
			}
		}

		public static void MaybeExportYamlForStructure(this IMonoBehaviour monoBehaviour, YAMLMappingNode node, IExportContainer container)
		{
			if (monoBehaviour.Structure != null)
			{
				YAMLMappingNode structureNode = (YAMLMappingNode)monoBehaviour.Structure.ExportYAML(container);
				node.Append(structureNode);
			}
		}

		public static IEnumerable<PPtr<IUnityObjectBase>> MaybeFetchDependenciesForStructure(this IMonoBehaviour monoBehaviour, DependencyContext context)
		{
			if (monoBehaviour.Structure != null)
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(monoBehaviour.Structure, monoBehaviour.Structure.Type.Name))
				{
					yield return asset;
				}
			}
		}
	}
}
