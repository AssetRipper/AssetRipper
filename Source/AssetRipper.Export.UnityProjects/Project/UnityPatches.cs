using System.Text;

namespace AssetRipper.Export.UnityProjects.Project;

public static class UnityPatches
{
	private const string RelativePathToPatchesDirectory = "Assets/Editor/AssetRipperPatches/";

	/// <summary>
	/// For some asset types, the complete recovery must be assisted by scripts that run in the Unity Editor.
	/// This method copies a script file from a <see cref="string"/> to the exported project.
	/// </summary>
	/// <param name="text">The text of the patch</param>
	/// <param name="name">The name of the patch</param>
	/// <param name="exportDirectoryPath">The path of the exported project</param>
	public static void ApplyPatchFromText(string text, string name, string exportDirectoryPath, FileSystem fileSystem)
	{
		string patchFileName = $"{name}.cs";
		string patchDirectoryPath = fileSystem.Path.Join(exportDirectoryPath, RelativePathToPatchesDirectory);
		string patchFilePath = fileSystem.Path.Join(patchDirectoryPath, patchFileName);
		if (fileSystem.File.Exists(patchFilePath))
		{
			return;
		}

		fileSystem.Directory.Create(patchDirectoryPath);
		fileSystem.File.WriteAllBytes(patchFilePath, Encoding.UTF8.GetBytes(text));
	}

	public static void ApplyBuiltinEditorScripts(string exportDirectoryPath, FileSystem fileSystem)
	{
		ApplyPatchFromText(MissingScriptsFinderText, "MissingScriptsFinder", exportDirectoryPath, fileSystem);
	}

	private static string MissingScriptsFinderText =>
		"""
		// ReSharper disable all
		// **************************************************************** //
		//
		//   Copyright (c) RimuruDev. All rights reserved.
		//   Contact:
		//          - Gmail:    rimuru.dev@gmail.com
		//          - GitHub:   https://github.com/RimuruDev
		//          - LinkedIn: https://www.linkedin.com/in/rimuru/
		//
		// **************************************************************** //
		
		#if UNITY_EDITOR
		using System;
		using System.Collections.Generic;
		using UnityEditor;
		using UnityEditor.SceneManagement;
		using UnityEngine;
		using Object = UnityEngine.Object;
		
		namespace AbyssMoth
		{
		    public sealed class MissingScriptsFinder : EditorWindow
		    {
		        private enum Tab { Scene, Prefabs }
		        private Tab currentTab = Tab.Scene;
		
		        [Serializable]
		        private struct ResultEntry
		        {
		            public string Name;
		            public string Path;
		            public int MissingCount;
		            public Object Target;
		            public bool IsPrefab;
		        }
		
		        private readonly List<ResultEntry> sceneResults = new();
		        private readonly List<ResultEntry> prefabResults = new();
		        
		        private Vector2 scrollPos;
		        private string searchFilter = "";
		        private bool searchSelectionOnly = false;
		        private bool autoSavePrefabs = true;
		
		        [MenuItem("Updated by StevenVR Tools/Find Missing Scripts")]
		        public static void ShowWindow() => GetWindow<MissingScriptsFinder>("Missing Scripts Finder");
		
		        private void OnGUI()
		        {
		            DrawHeader();
		            DrawToolbar();
		            DrawSettings();
		
		            EditorGUILayout.Space(5);
		
		            if (currentTab == Tab.Scene)
		                DrawSceneTab();
		            else
		                DrawPrefabsTab();
		
		            DrawResultsList(currentTab == Tab.Scene ? sceneResults : prefabResults);
		        }
		
		        private void DrawHeader()
		        {
		            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		            EditorGUILayout.LabelField("Missing Scripts Finder", EditorStyles.boldLabel);
		            EditorGUILayout.LabelField("Find and remove 'Missing (MonoBehaviour)' references efficiently.", EditorStyles.miniLabel);
		            EditorGUILayout.EndVertical();
		        }
		
		        private void DrawToolbar()
		        {
		            currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new[] { "Scene Search", "Project Prefabs" });
		        }
		
		        private void DrawSettings()
		        {
		            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		            searchFilter = EditorGUILayout.TextField("Filter Results", searchFilter);
		            
		            if (currentTab == Tab.Scene)
		                searchSelectionOnly = EditorGUILayout.Toggle("Search Selection Only", searchSelectionOnly);
		            
		            if (currentTab == Tab.Prefabs)
		                autoSavePrefabs = EditorGUILayout.Toggle("Auto-Save Prefabs", autoSavePrefabs);
		
		            EditorGUILayout.EndVertical();
		        }
		
		        private void DrawSceneTab()
		        {
		            EditorGUILayout.BeginHorizontal();
		            if (GUILayout.Button("Find in Scene", GUILayout.Height(30))) FindInScene();
		            GUI.color = new Color(1f, 0.4f, 0.4f);
		            if (GUILayout.Button("Clean Scene", GUILayout.Height(30))) CleanScene();
		            GUI.color = Color.white;
		            EditorGUILayout.EndHorizontal();
		        }
		
		        private void DrawPrefabsTab()
		        {
		            EditorGUILayout.BeginHorizontal();
		            if (GUILayout.Button("Scan All Prefabs", GUILayout.Height(30))) FindInPrefabs();
		            GUI.color = new Color(1f, 0.4f, 0.4f);
		            if (GUILayout.Button("Clean All Prefabs", GUILayout.Height(30))) CleanAllPrefabs();
		            GUI.color = Color.white;
		            EditorGUILayout.EndHorizontal();
		        }
		
		        private void DrawResultsList(List<ResultEntry> results)
		        {
		            if (results.Count == 0)
		            {
		                EditorGUILayout.HelpBox("No missing scripts found or search not performed yet.", MessageType.Info);
		                return;
		            }
		
		            EditorGUILayout.LabelField($"Results ({results.Count})", EditorStyles.boldLabel);
		            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
		
		            for (int i = 0; i < results.Count; i++)
		            {
		                var entry = results[i];
		                if (!string.IsNullOrEmpty(searchFilter) && !entry.Name.ToLower().Contains(searchFilter.ToLower()))
		                    continue;
		
		                EditorGUILayout.BeginHorizontal(EditorStyles.miniButton);
		                
		                var icon = EditorGUIUtility.IconContent(entry.IsPrefab ? "Prefab Icon" : "GameObject Icon");
		                GUILayout.Label(icon, GUILayout.Width(20));
		
		                if (GUILayout.Button($"{entry.Name} ({entry.MissingCount} missing)", EditorStyles.label))
		                {
		                    EditorGUIUtility.PingObject(entry.Target);
		                    Selection.activeObject = entry.Target;
		                }
		
		                if (GUILayout.Button("Fix", GUILayout.Width(40)))
		                {
		                    RemoveFromSingle(entry);
		                    results.RemoveAt(i);
		                    return;
		                }
		
		                EditorGUILayout.EndHorizontal();
		            }
		
		            EditorGUILayout.EndScrollView();
		        }
		
		        private void FindInScene()
		        {
		            sceneResults.Clear();
		            var transforms = GetSceneScope();
		
		            foreach (var t in transforms)
		            {
		                if (t == null) continue;
		                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
		                if (count > 0)
		                {
		                    sceneResults.Add(new ResultEntry
		                    {
		                        Name = t.name,
		                        Path = GetFullPath(t.gameObject),
		                        MissingCount = count,
		                        Target = t.gameObject,
		                        IsPrefab = false
		                    });
		                }
		            }
		        }
		
		        private void CleanScene()
		        {
		            int totalFixed = 0;
		            foreach (var entry in sceneResults)
		            {
		                if (entry.Target is GameObject go)
		                {
		                    Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
		                    totalFixed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
		                }
		            }
		            
		            EditorSceneManager.MarkAllScenesDirty();
		            Debug.Log($"<color=cyan>[MissingScriptsFinder]</color> Removed {totalFixed} scripts from scene.");
		            sceneResults.Clear();
		        }
		
		        private void FindInPrefabs()
		        {
		            prefabResults.Clear();
		            var guids = AssetDatabase.FindAssets("t:Prefab");
		            
		            for (int i = 0; i < guids.Length; i++)
		            {
		                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
		                EditorUtility.DisplayProgressBar("Scanning Prefabs", path, (float)i / guids.Length);
		
		                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
		                if (prefab == null) continue;
		
		                int missingInPrefab = 0;
		                var components = prefab.GetComponentsInChildren<Component>(true);
		                foreach (var c in components) if (c == null) missingInPrefab++;
		
		                if (missingInPrefab > 0)
		                {
		                    prefabResults.Add(new ResultEntry
		                    {
		                        Name = prefab.name,
		                        Path = path,
		                        MissingCount = missingInPrefab,
		                        Target = prefab,
		                        IsPrefab = true
		                    });
		                }
		            }
		            EditorUtility.ClearProgressBar();
		        }
		
		        private void CleanAllPrefabs()
		        {
		            int totalFixed = 0;
		            try
		            {
		                for (int i = 0; i < prefabResults.Count; i++)
		                {
		                    var entry = prefabResults[i];
		                    EditorUtility.DisplayProgressBar("Cleaning Prefabs", entry.Path, (float)i / prefabResults.Count);
		                    
		                    GameObject root = PrefabUtility.LoadPrefabContents(entry.Path);
		                    int removed = 0;
		                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
		                    {
		                        removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
		                    }
		
		                    if (removed > 0)
		                    {
		                        totalFixed += removed;
		                        if (autoSavePrefabs)
		                            PrefabUtility.SaveAsPrefabAsset(root, entry.Path);
		                    }
		                    PrefabUtility.UnloadPrefabContents(root);
		                }
		            }
		            finally
		            {
		                EditorUtility.ClearProgressBar();
		                AssetDatabase.Refresh();
		                prefabResults.Clear();
		            }
		            Debug.Log($"<color=cyan>[MissingScriptsFinder]</color> Removed {totalFixed} scripts from Prefabs.");
		        }
		
		        private void RemoveFromSingle(ResultEntry entry)
		        {
		            if (entry.IsPrefab)
		            {
		                GameObject root = PrefabUtility.LoadPrefabContents(entry.Path);
		                foreach (var t in root.GetComponentsInChildren<Transform>(true))
		                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
		                
		                PrefabUtility.SaveAsPrefabAsset(root, entry.Path);
		                PrefabUtility.UnloadPrefabContents(root);
		            }
		            else
		            {
		                Undo.RegisterCompleteObjectUndo(entry.Target, "Remove Missing Script");
		                GameObjectUtility.RemoveMonoBehavioursWithMissingScript((GameObject)entry.Target);
		            }
		        }
		
		        private Transform[] GetSceneScope()
		        {
		            if (searchSelectionOnly)
		            {
		                var list = new List<Transform>();
		                foreach (var go in Selection.gameObjects)
		                    list.AddRange(go.GetComponentsInChildren<Transform>(true));
		                return list.ToArray();
		            }
		
		#if UNITY_6000_0_OR_NEWER
		            return FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		#else
		            return FindObjectsOfType<Transform>(true);
		#endif
		        }
		
		        private static string GetFullPath(GameObject go)
		        {
		            string path = go.name;
		            while (go.transform.parent != null)
		            {
		                go = go.transform.parent.gameObject;
		                path = go.name + "/" + path;
		            }
		            return path;
		        }
		    }
		}
		#endif
		""";
}
