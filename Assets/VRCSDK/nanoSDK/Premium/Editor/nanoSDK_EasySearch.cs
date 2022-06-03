using nanoSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using nanoSDK.Premium;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using nanoSDKHash;

namespace nanoSDK.Premium
{
    public class NanoSDK_EasySearch : EditorWindow
    {
        private static GUIStyle _vrcSdkHeader;
        private static Vector2 _changeLogScroll;
        private static string _searchString = "";

        private static int _sliderLeftValue = 10;

        //[MenuItem("nanoSDK/EasySearch", false, 501)]
        public static void OpenSplashScreen()
        {
            GetWindow<NanoSDK_EasySearch>(true);
            if (NanoApiManager.IsLoggedInAndVerified()) return;
            NanoApiManager.OpenLoginWindow();
        }

        public void OnGUI()
        {
            GUILayout.Box("", _vrcSdkHeader);
            GUILayout.Space(4);
            GUI.backgroundColor = Color.gray;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Check for Updates")) NanoApiManager.CheckServerVersion("latest");
            if (GUILayout.Button("Reinstall SDK"))
                NanoSDK_AutomaticUpdateAndInstall.DeleteAndDownloadAsync();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("nanoSDK Discord")) Application.OpenURL("https://nanosdk.net/discord");
            if (GUILayout.Button("nanoSDK Website")) Application.OpenURL("https://nanoSDK.net/");
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            GUILayout.FlexibleSpace();
            _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"),
                GUILayout.Width(780));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                _searchString = string.Empty;
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Asset Count: {_results.Count}", EditorStyles.boldLabel);
            _sliderLeftValue = EditorGUILayout.IntSlider(_sliderLeftValue, 1, 1000);
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Search"))
            {
                if (Process.GetProcessesByName("Everything").Length != 0) FillList();
                else
                {
                    if (EditorUtility.DisplayDialog("nanoSDK", "Search Everything isnt Running please make sure to run it.",
                            "Okay", "Install")) Close();
                    else RunInstallAction();
                }
            }

            EditorGUILayout.EndHorizontal();


            if (_results == null) return;

            _changeLogScroll = GUILayout.BeginScrollView(_changeLogScroll, GUILayout.Width(800));
            var resultCount = 0;
            foreach (var result in _results)
            {
                resultCount++;


                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                if (!result.Folder)
                {
                    GUILayout.Label(resultCount.ToString(), GUILayout.Width(50));
                    GUILayout.Label(result.Filename);
                    if (GUILayout.Button("Import", GUILayout.Width(50)))
                    {
                        AssetDatabase.ImportPackage(result.Path, true);
                    }
                }
                else
                {
                    GUILayout.Label(resultCount.ToString(), GUILayout.Width(50));
                    GUILayout.Label(result.Filename + " (Folder)");
                    if (GUILayout.Button("Goto", GUILayout.Width(50)))
                    {
                        Process.Start(result.Path);
                    }
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(3);
            }

            GUILayout.EndScrollView();
        }

        private readonly List<Everything.Result> _results = new List<Everything.Result>();

        private void FillList()
        {
            _results.Clear();
            _results.AddRange(Everything.Search($"{_searchString} endwith:.unitypackage", _sliderLeftValue));
        }

        private void RunInstallAction()
        {
            if (EditorUtility.DisplayDialog("nanoSDK",
                    "Download of Search Everything has to be made manually since its not a nanoSDK product.", "Okay",
                    "Open website")) Close();
            else Process.Start("https://www.voidtools.com/downloads/");
        }

        public void OnEnable()
        {

            nanoSDKCheckHashes.CheckHashes();
            titleContent = new GUIContent("EasySearch");

            maxSize = new Vector2(800, 820);
            minSize = maxSize;

            _vrcSdkHeader = new GUIStyle
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
