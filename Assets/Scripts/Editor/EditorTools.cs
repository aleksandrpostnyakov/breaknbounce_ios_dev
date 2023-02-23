using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EditorTools 
{
    [MenuItem("Tools/Bonanza/StartGame %`", priority = 0)]
    public static void StartGame() => OpenScene("Assets/Scenes/Loading.unity", true);
    
    [MenuItem("Tools/Bonanza/LoadingScene", priority = 2)]
    public static void LoadingScene() => OpenScene("Assets/Scenes/Loading.unity");
    
    [MenuItem("Tools/Bonanza/UIScene", priority = 2)]
    public static void UIScene() => OpenScene("Assets/Scenes/UiScene.unity");
    [MenuItem("Tools/Bonanza/LevelScene", priority = 2)]
    public static void LevelScene() => OpenScene("Assets/Scenes/LevelScene.unity");
    
    
    private static void OpenScene(string scenePath, bool play = false)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
            if (play) {
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
        }
    }
    
    [MenuItem("Tools/Bonanza/Clear/Clear All")]
    public static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
