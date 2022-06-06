using UnityEngine;
using System.IO;
using System;
using UnityEditor;
using System.Net.Http;
using System.Net;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace nanoSDK
{
    public class NanoUpdater : MonoBehaviour
    { //api features in here bc files will be delted when process is being made
        private static readonly HttpClient HttpClient = new HttpClient();

        private const string _BASE_URL = "https://api.nanosdk.net";
        
        private static readonly Uri _LatestSDKVersionUri = new Uri(_BASE_URL + $"/public/sdk/version");
        private static readonly Uri _SdkVersionUri = new Uri(_BASE_URL + "/public/sdk/version/list");
        
        public static string CurrentVersion { get;} = File.ReadAllText($"Assets{Path.DirectorySeparatorChar}VRCSDK{Path.DirectorySeparatorChar}version.txt").Replace("\n", "");
        public static List<SdkVersionBaseINTERNDATA> ServerVersionList;
        public static SdkVersionBaseINTERNDATA LatestVersion { set; get; }
        public static SdkVersionBaseINTERNDATA LatestBetaVersion { set; get; }


        //select where to be imported (sdk)
        private static string assetPath = $"Assets{Path.DirectorySeparatorChar}";
        //Custom name for downloaded unitypackage
        private static string assetName = "unitypackage";
        //gets VRCSDK Directory Path
        private static string vrcsdkPath = $"Assets{Path.DirectorySeparatorChar}VRCSDK{Path.DirectorySeparatorChar}";


        //[MenuItem("nanoSDK/Update Test", false, 500)]
        public static async void DeleteAndDownloadAsync(string version = "latest")
        {
            using (WebClient w = new WebClient())
            {
                w.Headers.Set(HttpRequestHeader.UserAgent, "Webkit Gecko wHTTPS (Keep Alive 55)");
                w.DownloadProgressChanged += FileDownloadProgress;
                try
                {
                    string url = await GetUrlFromVersion(version);
                    if (url == null) throw new Exception("Invalid version");
                    await w.DownloadFileTaskAsync(new Uri(url), Path.GetTempPath() + Path.DirectorySeparatorChar + $"{version}.{assetName}");
                }
                catch (Exception ex)
                {
                    NanoLog("Download failed!");
                    if (EditorUtility.DisplayDialog("nanoSDK_Automatic_DownloadAndInstall", "nanoSDK Failed Download: " + ex.Message, "Join Discord for help", "Cancel"))
                    {
                        Application.OpenURL("https://nanosdk.net/discord");
                    }
                    return;
                }
            }
            NanoLog("Download Complete");

            try
            {

                if (EditorUtility.DisplayDialog("nanoSDK_Automatic_DownloadAndInstall", "The Old SDK will Be Deleted and the New SDK Will be imported!", "Okay", "Cancel"))
                {
                    // Shutting down DiscordRPC to fix problems while removing
                    DiscordRpc.Shutdown();
                    

                    NanoLog("Getting Files.....");
                    //gets every file in VRCSDK folder
                    string[] vrcsdkDir = Directory.GetFiles(vrcsdkPath, "*.*", SearchOption.AllDirectories);
                   
                    
                    Debug.Log("Deleting Files...");

                    //Deletes All Files in VRCSDK folder
                    await Task.Run(() =>
                    {
                        foreach (string f in vrcsdkDir)
                        {
                            if (!IsDLLFile(f))
                            {
                                NanoLog($"{f} - Deleted");
                                File.Delete(f);
                            }
                        }
                        string[] dllDir = Directory.GetFiles(vrcsdkPath, "*.dll", SearchOption.AllDirectories);
                        foreach (string f in dllDir)
                        {
                            try
                            {
                                NanoLog($"{f} - Deleted");
                                File.Delete(f);
                            }
                            catch (Exception) { }
                        }
                    });
                }
                else
                {
                    NanoLog("User declined update");
                    
                    NanoLog("Deleting downloaded file");
                    File.Delete(Path.GetTempPath() + Path.DirectorySeparatorChar + $"{version}.{assetName}");
                    NanoLog("finished deletion of downloaded file");
                    return;
                    
                }
            }
            catch (DirectoryNotFoundException)
            {
                EditorUtility.DisplayDialog("Error Deleting Files", "Error wihle trying to find VRCSDK Folder.", "Ignore");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error Deleting VRCSDK", ex.Message, "Okay");
                return;
            }

            try
            {
                AssetDatabase.ImportPackage(Path.GetTempPath() + Path.DirectorySeparatorChar + $"{version}.{assetName}", false);

            }
            catch (Exception ex)
            {

                NanoLog("Download failed!");
                if (EditorUtility.DisplayDialog("nanoSDK_Automatic_DownloadAndInstall", "nanoSDK Failed Download: " + ex.Message, "Join Discord for help", "Cancel"))
                {
                    Application.OpenURL("https://nanosdk.net/discord");
                }
            }

            AssetDatabase.Refresh();

        }
        public static async Task UpdateVersionData()
        {
            ServerVersionList = await GetVersionList();
            LatestVersion = await GetLatestVersion(SdkVersionBaseINTERNDATA.ReleaseType.World, SdkVersionBaseINTERNDATA.BranchType.Release);
            LatestBetaVersion = await GetLatestVersion(SdkVersionBaseINTERNDATA.ReleaseType.World, SdkVersionBaseINTERNDATA.BranchType.Beta);
        }
        public static async Task<SdkVersionBaseINTERNDATA> GetLatestVersion(SdkVersionBaseINTERNDATA.ReleaseType type, SdkVersionBaseINTERNDATA.BranchType branch)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_LatestSDKVersionUri + $"?type={type}&branch={branch}")
            };
            using (var response = await HttpClient.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SdkVersionBaseINTERN<SdkVersionBaseINTERNDATA>>(data).Data;
                    
                }
                NanoLog("Failed to get latest version");
                return null;
            }
        }
        public static async Task<List<SdkVersionBaseINTERNDATA>> GetVersionList()
        {

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = _SdkVersionUri
            };

            using (var response = await HttpClient.SendAsync(request))
            {
                string result = await response.Content.ReadAsStringAsync();
                var SERVERCHECKproperties = JsonConvert.DeserializeObject<SdkVersionBaseINTERN<List<SdkVersionBaseINTERNDATA>>>(result);
                return removeEntries(SERVERCHECKproperties.Data, SdkVersionBaseINTERNDATA.ReleaseType.World);
            } //without AuthKey Sending

        }
        private static List<SdkVersionBaseINTERNDATA> removeEntries(List<SdkVersionBaseINTERNDATA> list, SdkVersionBaseINTERNDATA.ReleaseType release)
        {
            List<SdkVersionBaseINTERNDATA> newList = new List<SdkVersionBaseINTERNDATA>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == release) continue;
                newList.Add(list[i]);
            }
            return newList;
        }
        private async static Task<string> GetUrlFromVersion(string version)
        {
            ServerVersionList = await GetVersionList();
            string url = null;
            if (version.Equals("latest")) url = ServerVersionList[0].Url;
            else if (version.Equals("beta")) url = ServerVersionList[ServerVersionList.Count - 1].Url;

            for (int i = 0; i < ServerVersionList.Count; i++)
            {
                if (version.Equals(ServerVersionList[i].Version)) url = ServerVersionList[i].Url;
            }
            return url;
        }
        private static void FileDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            //Creates A ProgressBar
            var progress = e.ProgressPercentage;
            if (progress < 0) return;
            if (progress >= 100)
            {
                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar("Download of " + assetName,
                    "Downloading " + assetName + " " + progress + "%",
                    (progress / 100F));
            }
        }
        private static bool IsDLLFile(string path)
        {
            if (path.Substring(path.Length - 3).Equals("dll")) return true;
            return false;
        }
        private static void NanoLog(string message)
        {
            //Our Logger

            message = "<color=magenta>" + message + "</color>";

            Debug.Log("[nanoSDK] AssetDownloadManager: " + message);
            message = "<color=white>" + message + "</color>";
        }
    }

    public class SdkVersionBaseINTERNDATA
    {
        public string Url { get; set; }
        public string Version { get; set; }
        public ReleaseType Type { get; set; }

        public BranchType Branch { get; set; }

        public enum ReleaseType
        {
            Avatar = 0,
            World = 1
        }

        public enum BranchType
        {
            Release = 0,
            Beta = 1
        }
    }

    public class SdkVersionBaseINTERN<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
