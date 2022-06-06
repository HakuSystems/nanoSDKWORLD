using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using nanoSDKHash;

namespace nanoSDK
{
    class nanoSDK_Manage : EditorWindow
    {

        // Login
        private string userInputText;
        private string passInputText;

        // Register
        private string regUserInputText;
        private string regPassInputText;
        private string regEmailInputText;

        // License
        private string redeemCode;

        //Window stuff
        private static GUIStyle nanoSdkHeader;
        private static readonly int _sizeX = 1200;
        private static readonly int _sizeY = 800;
        public string _webData;
        private static Vector2 changeLogScroll;

        //Switch things
        int toolbarInt = 1;
        string[] toolbarStrings = { "Changelogs", "Settings", "Importables" };
        private bool runChangelog;

        //Migrated from settings
        public static string projectConfigPath = "Assets/VRCSDK/nanoSDK/Configs/";
        private readonly string backgroundConfig = "BackgroundVideo.txt";
        private static readonly string projectDownloadPath = "Assets/VRCSDK/nanoSDK/Assets/";

        //Migrated from Importables
        private static readonly Dictionary<string, string> assets = new Dictionary<string, string>();
        private static Vector2 _importLogScroll;

        public nanoSDK_Manage(){}

        [MenuItem("nanoSDK/Manage", false, 100)]
        public static void OpenManageWindow()
        {
            GetWindow<nanoSDK_Manage>(true);
            if (NanoApiManager.IsLoggedInAndVerified()) return;
            NanoApiManager.OpenLoginWindow();
        }

        public async void OnEnable()
        {
            nanoSDKCheckHashes.CheckHashes();
            titleContent.text = "nanoSDK";
            maxSize = new Vector2(_sizeX, _sizeY);
            minSize = maxSize;

            if (!EditorPrefs.HasKey("nanoSDK_discordRPC"))
            {
                EditorPrefs.SetBool("nanoSDK_discordRPC", true);
            }

            if (!File.Exists(projectConfigPath + backgroundConfig) || !EditorPrefs.HasKey("nanoSDK_background"))
            {
                EditorPrefs.SetBool("nanoSDK_background", false);
                File.WriteAllText(projectConfigPath + backgroundConfig, "False");
            }

            NanoSDK_ImportManager.CheckForConfigUpdate();
            LoadJson();

            nanoSdkHeader = new GUIStyle
            {
                normal =
                    {
                    //Top
                       background = Resources.Load("nanoSDKLogo") as Texture2D,
                       textColor = Color.white
                    },
                fixedHeight = 110
            };

            await NanoUpdater.UpdateVersionData();

        }
        private void OnLostFocus()
        {
            if (NanoApiManager.IsLoggedInAndVerified()) return;
            NanoApiManager.OpenLoginWindow();
        }

        public async void OnGUI()
        {
            if (NanoApiManager.IsLoggedInAndVerified())
            {
                InitializeData();
            }
            
            GUILayout.BeginHorizontal();
            GUI.Box(new Rect(920, -20, 300, 0), "", nanoSdkHeader);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            toolbarInt = GUI.Toolbar(new Rect(500, 770, 250, 30), toolbarInt, toolbarStrings);

            switch (toolbarInt)
            {
                case 0:
                    ShowChangelogs();
                    break;

                case 1:
                    ShowSettings();
                    break;

                case 2:
                    ShowImportables();
                    break;
            }
            GUILayout.EndVertical();

            #region bottom Section
            GUILayout.BeginHorizontal();
            try
            {
                if (EditorGUI.DropdownButton(new Rect(10, 755, 105, 20), new GUIContent("Switch Version", "Want to Downgrade?"), FocusType.Passive))
                {
                    EditorUtility.DisplayDialog("nanoSDK", "Since this is the First World SDK, there is no other Version for world.", "OK");
                    /*
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("(Latest) Stable "+ NanoUpdater.LatestVersion.Version), false, HandleVersionItemClickedAsync, 1);
                    menu.AddItem(new GUIContent("(Latest) Beta " + NanoUpdater.LatestBetaVersion.Version), false, HandleVersionItemClickedAsync, 2);
                    menu.AddItem(new GUIContent("(Others)"), false, HandleVersionItemClickedAsync, 3);
                    menu.DropDown(new Rect(10, 755, 105, 20));
                    */
                }

                GUI.Label(new Rect(10, 775, 150, 20), NanoUpdater.CurrentVersion.Replace(';', ' '));
                if (NanoApiManager.User.IsPremium)
                {
                    if (EditorGUI.DropdownButton(new Rect(155, 775, 120, 20), new GUIContent("Manage Premium", "Select What window will be Shown"), FocusType.Passive))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("EasySearch"), false, HandlePremiumItemClicked, 1);
                        menu.AddItem(new GUIContent("nanoLoader"), false, HandlePremiumItemClicked, 2);
                        menu.DropDown(new Rect(155, 775, 120, 20));
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(155, 775, 120, 20), new GUIContent("Purchase Premium", "opens your browser!")))
                    {
                        string patreonUrl = "https://www.patreon.com/nanoSDK";
                        Application.OpenURL(patreonUrl);
                    }
                    if (GUI.Button(new Rect(270, 775, 20, 20), new GUIContent("?", "if you would like to support us with our efforts to make nanoSDK the best it can be, you can Purchase Premium")))
                    {
                        if (EditorUtility.DisplayDialog("Premium",
                            "nanoSDK Premium is billed Monthly and gives you more Features(read more in our discord server)",
                            "Join Discord Server", "Cancel"))
                        {
                            Application.OpenURL("https://nanosdk.net/discord");
                        }
                    }
                }
                GUI.Label(new Rect(1110, 775, 100, 20), "nanoSDK.net");
                GUI.contentColor = Color.green;
                GUI.Label(new Rect(530, 750, 200, 20), "Thanks for Choosing nanoSDK!");

            }
            catch (NullReferenceException)
            {
                await NanoUpdater.UpdateVersionData();
                Repaint();
            }
            GUILayout.EndHorizontal();
            #endregion

            /* save for wann auch immer
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button("Check for Updates"))
            {
                NanoApiManager.CheckServerVersion("latest");
            }
            if (GUILayout.Button("Reinstall SDK"))
            {
                await 
            
            NanoUpdater.DeleteAndDownloadAsync();
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
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            GUILayout.Space(2);
            */
        }
        void HandlePremiumItemClicked(object item)
        {
            switch (item)
            {
                case 1: //EasySearch
                    Premium.NanoSDK_EasySearch.OpenSplashScreen();
                    break;
                case 2: //nanoLoader
                    Premium.NanoLoader.OpenSplashScreen();
                    break;
                default:
                    break;
            }
        }
        void HandleVersionItemClickedAsync(object item)
        {
            switch (item)
            {
                case 1: //Release
                    if (EditorUtility.DisplayDialog("Release", "Do you want to download the latest Release Version?", "OK", "Cancel"))
                    {
                        NanoUpdater.DeleteAndDownloadAsync("latest");
                    }
                    break;
                case 2: //Beta
                    if (EditorUtility.DisplayDialog("Beta", "Do you want to download the latest Beta Version?", "OK", "Cancel"))
                    {
                       NanoUpdater.DeleteAndDownloadAsync("beta");
                    }
                    break;
                case 3: //Others
                    NanoSDKOtherVersions window = (NanoSDKOtherVersions)EditorWindow.GetWindow(typeof(NanoSDKOtherVersions));
                    window.Show();

                    break;
                default:
                    break;
            }
        }
        #region Importables
        private void ShowImportables()
        {
            if (NanoApiManager.IsLoggedInAndVerified())
            {
                GUILayout.Space(50);
                //Update assets
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Update assets (config)"))
                {
                    NanoSDK_ImportManager.UpdateConfig();
                }
                GUILayout.EndHorizontal();

                //Imports V!V

                _importLogScroll = GUILayout.BeginScrollView(_importLogScroll, GUILayout.Width(_sizeX));
                foreach (var asset in assets)
                {
                    GUILayout.BeginHorizontal();
                    if (asset.Value == "")
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(asset.Key);
                        GUILayout.FlexibleSpace();
                    }
                    else
                    {
                        if (GUILayout.Button(
                            (File.Exists(GetAssetPath() + asset.Value) ? "Import" : "Download") +
                            " " + asset.Key))
                        {
                            NanoSDK_ImportManager.DownloadAndImportAssetFromServer(asset.Value);
                        }

                        if (GUILayout.Button("Del", GUILayout.Width(40)))
                        {
                            NanoSDK_ImportManager.DeleteAsset(asset.Value);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();


            }
        }

        public static void LoadJson()
        {
            assets.Clear();

            dynamic configJson =
                JObject.Parse(File.ReadAllText(projectConfigPath + NanoSDK_ImportManager.configName));

            //Debug.Log("Server Asset Url is: " + configJson["config"]["serverUrl"]);
            NanoSDK_ImportManager.serverUrl = configJson["config"]["serverUrl"].ToString();

            foreach (JProperty x in configJson["assets"])
            {
                var value = x.Value;

                var buttonName = "";
                var file = "";

                foreach (var jToken in value)
                {
                    var y = (JProperty)jToken;
                    switch (y.Name)
                    {
                        case "name":
                            buttonName = y.Value.ToString();
                            break;
                        case "file":
                            file = y.Value.ToString();
                            break;
                    }
                }
                assets[buttonName] = file;
            }
        }

        public static string GetAssetPath()
        {
            if (EditorPrefs.GetBool("nanoSDK_onlyProject", false))
            {
                return projectDownloadPath;
            }

            var assetPath = EditorPrefs.GetString("nanoSDK_customAssetPath", "%appdata%/nanoSDK/")
                .Replace("%appdata%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                .Replace("/", "\\");

            if (!assetPath.EndsWith("\\"))
            {
                assetPath += "\\";
            }

            Directory.CreateDirectory(assetPath);
            return assetPath;
        }


        #endregion
        #region Settings
        private void ShowSettings()
        {
            if (NanoApiManager.IsLoggedInAndVerified())
            {
                

                GUI.Label(new Rect(580, 155, 100, 20), "Overall:");
                GUILayout.BeginHorizontal();
                var isDiscordEnabled = EditorPrefs.GetBool("nanoSDK_discordRPC", true);
                var enableDiscord = EditorGUI.ToggleLeft(new Rect(560, 175, 100, 20), "Discord RPC", isDiscordEnabled);
                if (enableDiscord != isDiscordEnabled)
                {
                    EditorPrefs.SetBool("nanoSDK_discordRPC", enableDiscord);
                    EnableCloseMessage();
                }
                //Hide Console logs
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                var isHiddenConsole = EditorPrefs.GetBool("nanoSDK_HideConsole");
                var enableConsoleHide = EditorGUI.ToggleLeft(new Rect(560, 195, 200, 20), "Hide Console Errors", isHiddenConsole);
                if (enableConsoleHide == true)
                {
                    EditorPrefs.SetBool("nanoSDK_HideConsole", true);
                    Debug.ClearDeveloperConsole();
                    Debug.unityLogger.logEnabled = false;
                }
                else if (enableConsoleHide == false)
                {
                    EditorPrefs.SetBool("nanoSDK_HideConsole", false);
                    Debug.ClearDeveloperConsole();
                    Debug.unityLogger.logEnabled = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUI.Label(new Rect(580, 215, 200, 20), "Upload panel:");
                GUILayout.BeginHorizontal();
                var isBackgroundEnabled = EditorPrefs.GetBool("nanoSDK_background", false);
                var enableBackground = EditorGUI.ToggleLeft(new Rect(560, 235, 200, 20), "Custom background", isBackgroundEnabled);
                if (enableBackground != isBackgroundEnabled)
                {
                    EditorPrefs.SetBool("nanoSDK_background", enableBackground);
                    File.WriteAllText(projectConfigPath + backgroundConfig, enableBackground.ToString());
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUI.Label(new Rect(580, 255, 200, 20),"Import panel:");
                GUILayout.BeginHorizontal();
                var isOnlyProjectEnabled = EditorPrefs.GetBool("nanoSDK_onlyProject", false);
                var enableOnlyProject = EditorGUI.ToggleLeft(new Rect(560, 275, 200, 20), "Save files only in project", isOnlyProjectEnabled);
                if (enableOnlyProject != isOnlyProjectEnabled)
                {
                    EditorPrefs.SetBool("nanoSDK_onlyProject", enableOnlyProject);
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUI.Label(new Rect(560, 295, 200, 20), "Asset path:");
                GUILayout.BeginHorizontal();
                var customAssetPath = EditorGUI.TextField(new Rect(560, 315, 200, 20), "",
                    EditorPrefs.GetString("nanoSDK_customAssetPath", "%appdata%/nanoSDK/"));
                if (GUI.Button(new Rect(560, 340, 60, 20), "Choose"))
                {
                    var path = EditorUtility.OpenFolderPanel("Asset download folder",
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nanoSDK");
                    if (path != "")
                    {
                        Debug.Log(path);
                        customAssetPath = path;
                    }
                }

                if (GUI.Button(new Rect(700, 340, 60, 20), "Reset"))
                {
                    customAssetPath = "%appdata%/nanoSDK/";
                }

                if (EditorPrefs.GetString("nanoSDK_customAssetPath", "%appdata%/nanoSDK/") != customAssetPath)
                {
                    EditorPrefs.SetString("nanoSDK_customAssetPath", customAssetPath);
                }
                GUILayout.EndHorizontal();


            }
        }

        public static void EnableCloseMessage()
        {
            if (EditorPrefs.GetBool("nanoSDK_discordRPC"))
            {
                //Debug.Log("ON");
                if (EditorUtility.DisplayDialog("Discord RPC Restart", "To change Discord RPC you must restart unity  WARNING! Make sure you saved everything.", "Close Unity", "Cancel"))
                {
                    //Debug.Log("set to on");
                    EditorPrefs.SetBool("nanoSDK_discordRPC", true);
                    RealCloseProgram();
                }
                else
                {
                    //Debug.Log("set to off");
                    EditorPrefs.SetBool("nanoSDK_discordRPC", false);
                }
            }
            else
            {
                //Debug.Log("OFF");
                if (EditorUtility.DisplayDialog("Discord RPC Restart", "To change Discord RPC you must restart unity  WARNING! Make sure you saved everything.", "Close Unity", "Cancel"))
                {
                    //Debug.Log("set to off");
                    EditorPrefs.SetBool("nanoSDK_discordRPC", false);
                    RealCloseProgram();
                }
                else
                {
                    //Debug.Log("set to on");
                    EditorPrefs.SetBool("nanoSDK_discordRPC", true);
                }
            }
        }
        private static void RealCloseProgram()
        {
            NanoLog("Closing Unity");
            EditorApplication.Exit(0);
        }

        #endregion

        #region changelogs
        private void ShowChangelogs()
        {
            if (NanoApiManager.IsLoggedInAndVerified())
            {
                if (!runChangelog)
                {
                    GUILayout.TextArea("Loading Changelogs");
                    ReadChangelogs();
                    runChangelog = true;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(300); //Moshiro Move

                changeLogScroll = GUILayout.BeginScrollView(changeLogScroll,GUILayout.Height(500), GUILayout.Width(700));
                GUI.contentColor = Color.white;
                GUILayout.Space(5);
                GUILayout.TextArea(_webData);

                GUILayout.EndScrollView();

                GUILayout.EndHorizontal();
            }
        }

        private void ReadChangelogs()
        {
            NanoLog("Loaded Changelogs!");
            string url = "https://nanosdk.net/download/changelogs/Worldlogs.txt";
            using (var client = new WebClient())
            {
                var webData = client.DownloadString(url);
                _webData = webData;
            }
        }
        #endregion
        private static void NanoLog(string message)
        {
            //Our Logger

            message = "<color=magenta>" + message + "</color>";

            Debug.Log("[nanoSDK] Manage: " + message);
            message = "<color=white>" + message + "</color>";
        }
        private void InitializeData()
        {
            GUILayout.BeginVertical();
            //will show when user is logged in
            EditorGUILayout.LabelField($"ID:  {NanoApiManager.User.ID}");
            EditorGUILayout.LabelField($"Logged in as:  {NanoApiManager.User.Username}");
            EditorGUILayout.LabelField($"Email:  {NanoApiManager.User.Email}");
            EditorGUILayout.LabelField($"Permission: {NanoApiManager.User.Permission}");
            EditorGUILayout.LabelField($"Verified:  {NanoApiManager.User.IsVerified}");
            EditorGUILayout.LabelField($"Premium:  {NanoApiManager.User.IsPremium}");
            if (GUI.Button(new Rect(115, 75, 100, 20), "Copy"))
            {
                
                string copyContent = $@"
Username: {NanoApiManager.User.Username}
Email: {NanoApiManager.User.Email}
ID: {NanoApiManager.User.ID}
                    ";
                EditorGUIUtility.systemCopyBuffer = copyContent;
                NanoLog("Copyied!");
            }
            if (GUI.Button(new Rect(115, 100, 100, 20), "Logout"))
            {
                NanoApiManager.Logout();
                NanoLog("Logged out!");
            }
            GUILayout.EndVertical();
        }
    }
}
