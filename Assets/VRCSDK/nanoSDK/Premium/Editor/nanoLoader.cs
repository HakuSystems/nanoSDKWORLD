using nanoSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using nanoSDKHash;
using UnityEditor.SceneManagement;

namespace nanoSDK.Premium
{
    public class NanoLoader : EditorWindow
    {
        private static GUIStyle vrcSdkHeader;
        public static AssetBundle _bundle;
        public static GameObject _object;

        //[MenuItem("nanoSDK/nanoLoader", false, 501)]
        public static void OpenSplashScreen()
        {
            GetWindow<NanoLoader>(true);
            if (NanoApiManager.IsLoggedInAndVerified()) return;
            NanoApiManager.OpenLoginWindow();
        }
        public void OnGUI()
        {
            GUILayout.Box("", vrcSdkHeader);
            GUILayout.Space(4);
            GUI.backgroundColor = Color.gray;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Check for Updates"))
            {
                NanoApiManager.CheckServerVersion("latest");
            }
            if (GUILayout.Button("Reinstall SDK"))
            {
                NanoSDK_AutomaticUpdateAndInstall.DeleteAndDownloadAsync();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("nanoSDK Discord"))
            {
                Application.OpenURL("https://nanosdk.net/discord");
            }

            if (GUILayout.Button("nanoSDK Website"))
            {
                Application.OpenURL("https://nanoSDK.net/");
            }

            GUILayout.EndHorizontal();


            GUILayout.Label(
@"nanoLoader is a new feature that gives you control over
your asset bundles(.vrca files). 
Drag and drop your assetbundle into nanoLoader and it will load your World in
Unity with all of the shaders and everything else that your World has. 
This World can be seen in edit and play mode. 
The assets (specific files like soundfiles, meshes, or shaders) cannot be restored! 
This feature only gives you the ability to see
the World and how it was previously built in Unity. 
For example, you can use it to copy your lost World
settings and then paste the same settings onto your new World. 
For example, if you have forgotten what shader you
used on the old World, you can simply drag and drop your assetbundle
into nanoLoader and you will be able to see
all of the shaders and all of the animations. 
You can also play every single animation that the World has.
Like I mentioned before, assets will not be exported.", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Drag and drop your world.vrcw file here.");
            GUILayout.Space(60);
            GUILayout.Label("Note: this feature is only for Worlds it isnt for Avatars!");
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                if (DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0)
                {
                    foreach (string path in DragAndDrop.paths)
                    {
                        UnityEngine.Debug.Log("- " + path);
                        if (path.EndsWith(".vrcw"))
                        {
                            _bundle = AssetBundle.LoadFromFile(path);

                            if (_bundle.isStreamedSceneAssetBundle)
                            {
                                try
                                {
                                    string[] scenePaths = _bundle.GetAllScenePaths();
                                    string sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);

                                    if (EditorApplication.isPlaying)
                                    {
                                        SceneManager.LoadScene(sceneName);
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("Error", "You have to be in Playmode!", "Okay");
                                        _bundle.Unload(true);
                                    }
                                }
                                catch
                                {
                                    _bundle.Unload(true);
                                }
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("nanoLoader", "Sorry but this is not a World", "Okay");
                        }
                    }
                }
                EditorGUILayout.EndVertical();

            }

        }
        public void OnEnable()
        {

            nanoSDKCheckHashes.CheckHashes();
            titleContent = new GUIContent("nanoLoader");

            maxSize = new Vector2(500, 400);
            minSize = maxSize;

            vrcSdkHeader = new GUIStyle
            {
                normal =
                {
                    background = Resources.Load("") as Texture2D,
                    textColor = Color.white
                },
                fixedHeight = 1
            };
            
        }
    }
}
