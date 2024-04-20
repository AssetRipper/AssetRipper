using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_0;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Subclasses.ComponentPair;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Component;
using System.Diagnostics;

namespace AssetRipper.Import.Structure;

internal sealed partial record class GameInitializer
{
	private static class VersionChanger
	{
		public static void ChangeVersions(IEnumerable<AssetCollection> collections, UnityVersion targetVersion)
		{
			List<IUnityObjectBase> list = new();

			foreach (AssetCollection collection in collections)
			{
				if (collection.Version == targetVersion)
				{
					continue;
				}

				list.Clear();
				list.EnsureCapacity(collection.Count);
				list.AddRange(collection);
				foreach (IUnityObjectBase original in list)
				{
					if (original is not IObject)
					{
						continue;
					}

					IUnityObjectBase replacement = AssetFactory.Create(original.AssetInfo, targetVersion);
					if (original.GetType() == replacement.GetType())
					{
						continue;
					}

					replacement.CopyValues(original);
					HandleDifferingFields(original, replacement);
					collection.ReplaceAsset(replacement);
				}
			}
		}

		private static void HandleDifferingFields(IUnityObjectBase original, IUnityObjectBase replacement)
		{
			if (original is IRenderer renderer)
			{
				IRenderer newRenderer = (IRenderer)replacement;
				if (renderer.Has_StaticBatchInfo_C25())
				{
					if (newRenderer.Has_SubsetIndices_C25())
					{
						AssetList<uint> list = newRenderer.SubsetIndices_C25;
						list.AddRange(Enumerable.Range(renderer.StaticBatchInfo_C25.FirstSubMesh, renderer.StaticBatchInfo_C25.SubMeshCount).Select(i => (uint)i));
					}
				}
				else
				{
					if (newRenderer.Has_StaticBatchInfo_C25())
					{
						AssetList<uint> list = renderer.SubsetIndices_C25;
						unchecked
						{
							if (list.Count is > 0 and <= ushort.MaxValue && list[0] <= ushort.MaxValue)
							{
								bool compatible = true;
								for (int i = 0; i < list.Count - 1; i++)
								{
									if (list[i] + 1 != list[i + 1])
									{
										compatible = false;
										break;
									}
								}
								if (compatible)
								{
									newRenderer.StaticBatchInfo_C25.FirstSubMesh = (ushort)list[0];
									newRenderer.StaticBatchInfo_C25.SubMeshCount = (ushort)list.Count;
								}
							}
						}
					}
				}
			}
			else if (original is IMonoBehaviour monoBehaviour)
			{
				IMonoBehaviour newMonoBehaviour = (IMonoBehaviour)replacement;
				if (monoBehaviour.Structure is UnloadedStructure unloadedStructure)
				{
					newMonoBehaviour.Structure = new UnloadedStructure(newMonoBehaviour, unloadedStructure.AssemblyManager, unloadedStructure.StructureData);
				}
				else
				{
					newMonoBehaviour.Structure = monoBehaviour.Structure;
				}
			}
			else if (original is IGameObject gameObject)
			{
				IGameObject newGameObject = (IGameObject)replacement;
				PPtrConverter converter = new(gameObject, newGameObject);
				if (gameObject.Has_Component_AssetList_AssetPair_Int32_PPtr_Component_3_5())
				{
					if (newGameObject.Has_Component_AssetList_AssetPair_Int32_PPtr_Component_5())
					{
						foreach (AssetPair<int, PPtr_Component_3_5> pair in gameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_3_5)
						{
							AssetPair<int, PPtr_Component_5> newPair = newGameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_5.AddNew();
							newPair.Key = pair.Key;
							newPair.Value.CopyValues(pair.Value, converter);
						}
					}
					else if (newGameObject.Has_Component_AssetList_ComponentPair())
					{
						foreach (AssetPair<int, PPtr_Component_3_5> pair in gameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_3_5)
						{
							newGameObject.Component_AssetList_ComponentPair.AddNew().Component.CopyValues(pair.Value, converter);
						}
					}
				}
				else if (gameObject.Has_Component_AssetList_AssetPair_Int32_PPtr_Component_5())
				{
					if (newGameObject.Has_Component_AssetList_AssetPair_Int32_PPtr_Component_3_5())
					{
						foreach (AssetPair<int, PPtr_Component_5> pair in gameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_5)
						{
							AssetPair<int, PPtr_Component_3_5> newPair = newGameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_3_5.AddNew();
							newPair.Key = pair.Key;
							newPair.Value.CopyValues(pair.Value, converter);
						}
					}
					else if (newGameObject.Has_Component_AssetList_ComponentPair())
					{
						foreach (AssetPair<int, PPtr_Component_5> pair in gameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_5)
						{
							newGameObject.Component_AssetList_ComponentPair.AddNew().Component.CopyValues(pair.Value, converter);
						}
					}
				}
				else
				{
					Debug.Assert(gameObject.Has_Component_AssetList_ComponentPair());
					if (newGameObject.Has_Component_AssetList_AssetPair_Int32_PPtr_Component_3_5())
					{
						foreach (ComponentPair pair in gameObject.Component_AssetList_ComponentPair)
						{
							IComponent? component = gameObject.Collection.TryGetAsset<IComponent>(pair.Component);
							AssetPair<int, PPtr_Component_3_5> newPair = newGameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_3_5.AddNew();
							newPair.Key = component?.ClassID ?? (int)ClassIDType.Component;
							newPair.Value.SetAsset(newGameObject.Collection, component);
						}
					}
					else if (newGameObject.Has_Component_AssetList_AssetPair_Int32_PPtr_Component_5())
					{
						foreach (ComponentPair pair in gameObject.Component_AssetList_ComponentPair)
						{
							IComponent? component = gameObject.Collection.TryGetAsset<IComponent>(pair.Component);
							AssetPair<int, PPtr_Component_5> newPair = newGameObject.Component_AssetList_AssetPair_Int32_PPtr_Component_5.AddNew();
							newPair.Key = component?.ClassID ?? (int)ClassIDType.Component;
							newPair.Value.SetAsset(newGameObject.Collection, component);
						}
					}
				}
			}
		}
	}
}
