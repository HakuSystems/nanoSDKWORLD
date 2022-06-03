using UnityEngine;
using System.IO;
using System.Net;
using System;
using System.ComponentModel;
using UnityEditor;

namespace nanoSDK
{
    public class NanoSDK_ImportManager
    {
        public static string configName = "importConfig.json";
        public static string serverUrl = "https://nanoSDK.net/assets/";
        public static string internalServerUrl = "https://nanoSDK.net/assets/";

        public static void DownloadAndImportAssetFromServer(string assetName)
        {
            if (File.Exists(nanoSDK_Manage.GetAssetPath() + assetName))
            {
                NanoLog(assetName + " exists. Importing it..");
                ImportDownloadedAsset(assetName);
            }
            else
            {
                NanoLog(assetName + " does not exist. Starting download..");
                DownloadFile(assetName);
            }
        }

        private static void DownloadFile(string assetName)
        {
            WebClient w = new WebClient();
            w.Headers.Set(HttpRequestHeader.UserAgent, "Webkit Gecko wHTTPS (Keep Alive 55)");
            w.QueryString.Add("assetName", assetName);
            w.DownloadFileCompleted += FileDownloadCompleted;
            w.DownloadProgressChanged += FileDownloadProgress;
            string url = serverUrl + assetName;
            w.DownloadFileAsync(new Uri(url), nanoSDK_Manage.GetAssetPath() + assetName);
        }

        public static void DeleteAsset(string assetName)
        {
            File.Delete(nanoSDK_Manage.GetAssetPath() + assetName);
        }

        public static void UpdateConfig()
        {
            WebClient w = new WebClient();
            w.Headers.Set(HttpRequestHeader.UserAgent, "Webkit Gecko wHTTPS (Keep Alive 55)");
            w.DownloadFileCompleted += ConfigDownloadCompleted;
            w.DownloadProgressChanged += FileDownloadProgress;
            string url = internalServerUrl + configName;
            w.DownloadFileAsync(new Uri(url), nanoSDK_Manage.projectConfigPath + "update_" + configName);
        }

        private static void ConfigDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //var updateFile = File.ReadAllText(nanoSDK_Settings.projectConfigPath + "update_" + configName);
                File.Delete(nanoSDK_Manage.projectConfigPath + configName);
                File.Move(nanoSDK_Manage.projectConfigPath + "update_" + configName,
                    nanoSDK_Manage.projectConfigPath + configName);
                nanoSDK_Manage.LoadJson();

                EditorPrefs.SetInt("nanoSDK_configImportLastUpdated", (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                NanoLog("Import Config has been updated!");
            }
            else
            {
                NanoLog("Import Config could not be updated!");
            }
        }

        private static void FileDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string assetName = ((WebClient) sender).QueryString["assetName"];
            if (e.Error == null)
            {
                NanoLog("Download of file " + assetName + " completed!");
            }
            else
            {
                DeleteAsset(assetName);
                NanoLog("Download of file " + assetName + " failed!");
            }
        }

        private static void FileDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            var progress = e.ProgressPercentage;
            var assetName = ((WebClient) sender).QueryString["assetName"];
            if (progress < 0) return;
            if (progress >= 100)
            {
                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar("Download of " + assetName,
                    "Downloading " + assetName + ". Currently at: " + progress + "%",
                    (progress / 100F));
            }
        }

        public static void CheckForConfigUpdate()
        {
            if (EditorPrefs.HasKey("nanoSDK_configImportLastUpdated"))
            {
                var lastUpdated = EditorPrefs.GetInt("nanoSDK_configImportLastUpdated");
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (currentTime - lastUpdated < 3600)
                {
                    NanoLog("Not updating config due to much request, Time in seconds before last request: " + (currentTime - lastUpdated));
                    return;
                }
            }
            NanoLog("Updating import config");
            UpdateConfig();
        }

        private static void NanoLog(string message)
        {
            message = "<color=magenta>" + message + "</color>";

            Debug.Log("[nanoSDK] AssetDownloadManager: " + message);
            message = "<color=white>" + message + "</color>";
        }

        public static void ImportDownloadedAsset(string assetName)
        {
            AssetDatabase.ImportPackage(nanoSDK_Manage.GetAssetPath() + assetName, true);
        }
    }
}