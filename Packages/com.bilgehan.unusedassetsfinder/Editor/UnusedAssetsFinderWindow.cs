using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public class UnusedAssetsFinder : EditorWindow
{
    private List<string> unusedAssets = new List<string>();
    private Vector2 scrollPos;
    private bool excludeScripts = true;
    private bool excludeShaders = true;
    private bool onlyBuildScenes = true;
    private SceneAsset customScene;
        
    [MenuItem("Tools/Optimization/Unused Assets Finder")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetsFinder>("Unused Assets Finder");
    }

    void OnGUI()
    {
        GUILayout.Label("Unused Assets Finder", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        excludeScripts = EditorGUILayout.Toggle("Exclude .cs files", excludeScripts);
        excludeShaders = EditorGUILayout.Toggle("Exclude .shader files", excludeShaders);
        onlyBuildScenes = EditorGUILayout.Toggle("Only check scenes in Build Settings", onlyBuildScenes);

        if (!onlyBuildScenes)
        {
            customScene = (SceneAsset)EditorGUILayout.ObjectField("Specific Scene", customScene, typeof(SceneAsset), false);
        }
        
        if (GUILayout.Button("Scan Project", GUILayout.Height(30)))
        {
            ScanForUnusedAssets();
        }
        
        GUILayout.Space(10);

        if (unusedAssets.Count > 0)
        {
            GUILayout.Label($"Found <b>{unusedAssets.Count}</b> unused assets:", new GUIStyle(EditorStyles.label) { richText = true });
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(400));
            foreach (var asset in unusedAssets)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(asset);
                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(asset);
                    EditorGUIUtility.PingObject(obj);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (GUILayout.Button("Delete Selected", GUILayout.Height(30)))
            {
                DeleteUnusuedAssets();
            }
        }
        else
        {
            GUILayout.Label("No unused assets found yet. Click 'Scan Project' to start.");
        }
    }

    void ScanForUnusedAssets()
    {
        unusedAssets.Clear();
        
        string[] scenesToCheck;
        if (onlyBuildScenes)
        {
            scenesToCheck = EditorBuildSettings.scenes.Where(s => s.enabled)
                .Select(s => s.path).ToArray();
        }
        else if (customScene != null)
        {
            scenesToCheck = new[] { AssetDatabase.GetAssetPath(customScene) };
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "No scene selected.", "OK");
            return;
        }

        string[] allAssets = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/") && !p.Contains("/Editor/"))
            .ToArray();

        string[] allPossibleReferences = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.EndsWith(".unity") || p.EndsWith(".prefab") || p.EndsWith(".asset") 
                        || p.EndsWith(".mat") 
                        || p.EndsWith(".controller"))
            .ToArray();
        
        var excludeExtensions = new List<string>();
        if (excludeScripts)
            excludeExtensions.Add(".cs");
        if (excludeShaders)
            excludeExtensions.Add(".shader");
        
        allAssets = allAssets.Where(p => !excludeExtensions.Any(ext => p.EndsWith(ext))).ToArray();
        
        int checkedCount = 0;
        foreach (var asset in allAssets)
        {
            checkedCount++;

            if (EditorUtility.DisplayCancelableProgressBar("Scanning Assets", asset,
                    (float)checkedCount / allAssets.Length))
            {
                break;
            }
            
            bool isUsed = false;
            
            string guid = AssetDatabase.AssetPathToGUID(asset);

            foreach (var reference in allPossibleReferences)
            {
                string fileContent = File.ReadAllText(reference);
                if (fileContent.Contains(guid))
                {
                    isUsed = true;
                    break;
                }
            }

            if (!isUsed)
            {
                unusedAssets.Add(asset);
            }
        }
        EditorUtility.ClearProgressBar();
        Debug.Log($"Scan complete. Found <b>{unusedAssets.Count}</b> unused assets.");
    }

    void DeleteUnusuedAssets()
    {
        if (!EditorUtility.DisplayDialog("Delete Unused Assets",
                $"Are you sure you want to delete {unusedAssets.Count} assets?", "Yes", "No"))
            return;
        
        string recyclePath = "Assets/UnusedAssetsBackup";
        if (!Directory.Exists(recyclePath))
            Directory.CreateDirectory(recyclePath);

        foreach (var unusedAsset in unusedAssets)
        {
            string fileName = Path.GetFileName(unusedAsset);
            string destPath = Path.Combine(recyclePath, fileName);
            AssetDatabase.MoveAsset(unusedAsset, destPath);
        }
        
        AssetDatabase.Refresh();
        unusedAssets.Clear();
        
        EditorUtility.DisplayDialog("Done", "Unused assets deleted successfully.", "OK");
    }
}
