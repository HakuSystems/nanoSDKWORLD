using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace nanoSDK
{
    public class NanoSDKOtherVersions : EditorWindow
    {
        public List<SdkVersionBaseINTERNDATA> versionList;
        public string currentVersion;
        private static Vector2 scrollView;
        private static readonly int _sizeX = 500;
        private static readonly int _sizeY = 500;

        private bool runOnce;

        [Obsolete]
        private async void OnEnable()
        {
            title = "nanoSDK Other Versions";
            maxSize = new Vector2(_sizeX, _sizeY);
            minSize = maxSize;
            await GetVERSIONData();
            
        }

        private void OnGUI()
        {
            
            if (versionList == null)
                return;
            GUILayout.BeginVertical();
            GUILayout.Label(versionList.Count+" Other Versions", EditorStyles.boldLabel);
            GUILayout.EndVertical();
            scrollView = GUILayout.BeginScrollView(scrollView);

            foreach (var version in versionList)
            {
                //label with button
                GUILayout.BeginHorizontal();
                GUILayout.Label(version.Version+", "+version.Type+", "+version.Branch+":", EditorStyles.boldLabel);
                //download beta or stable
                if (GUILayout.Button("Download"))
                {
                    if (version.Branch == SdkVersionBaseINTERNDATA.BranchType.Beta)
                    {
                        if (EditorUtility.DisplayDialog("Download Beta Version", "Are you sure you want to download the beta version? This will overwrite your current version.", "Yes", "No"))
                        {
                            NanoUpdater.DeleteAndDownloadAsync(version.Version);
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("Download Stable Version", "Are you sure you want to download the stable version? This will overwrite your current version.", "Yes", "No"))
                        {
                            NanoUpdater.DeleteAndDownloadAsync(version.Version);
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            GUILayout.TextArea("Current Version: " + currentVersion.Replace(';', ' '), EditorStyles.boldLabel);
            if (GUILayout.Button("Reinstall SDK"))
            {
                string[] split = currentVersion.Split(';');
                string version = split[0];
                string type = split[1];
                string branch = split[2];
                NanoUpdater.DeleteAndDownloadAsync(version);
            }
            GUILayout.EndHorizontal();
        }


        private async Task GetVERSIONData()
        {
            if (!runOnce)
            {
                versionList = await NanoUpdater.GetVersionList();
                runOnce = true;
            }
            if (File.Exists($"Assets{Path.DirectorySeparatorChar}VRCSDK{Path.DirectorySeparatorChar}version.txt"))
            {
                var version = File.ReadAllText($"Assets{Path.DirectorySeparatorChar}VRCSDK{Path.DirectorySeparatorChar}version.txt");
                currentVersion = version;
            }
        }        

    }
}
