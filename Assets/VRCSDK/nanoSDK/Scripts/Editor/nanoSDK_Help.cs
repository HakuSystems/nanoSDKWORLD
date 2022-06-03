using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using nanoSDKHash;

namespace nanoSDK
{
    public class NanoSDK_Help
    {

        [MenuItem("nanoSDK/Help/Discord", false, 1049)]
        public static void OpenDiscordLink()
        {
            Application.OpenURL("https://nanoSDK.net/discord");
        }
        [MenuItem("nanoSDK/Premium/EasySearch", false, 1049)]
        public static void OpenEasySearch()
        {
            nanoSDKCheckHashes.CheckHashes();
            try
            {
                if (NanoApiManager.User.IsPremium && NanoApiManager.IsLoggedInAndVerified())
                {
                    Premium.NanoSDK_EasySearch.OpenSplashScreen();
                }
                else
                {
                    EditorUtility.DisplayDialog("Premium", "You need to be a premium user to use this feature.", "OK");
                }
            }
            catch (NullReferenceException)
            {
                EditorUtility.DisplayDialog("Premium", "You need to be logged in to use this feature.", "OK");
                NanoApiManager.OpenLoginWindow();
            }
            
        }
        [MenuItem("nanoSDK/Premium/nanoLoader", false, 1049)]
        public static void OpenNanoLoader()
        {
            nanoSDKCheckHashes.CheckHashes();
            try
            {

                if (NanoApiManager.User.IsPremium && NanoApiManager.IsLoggedInAndVerified())
                {
                    Premium.NanoLoader.OpenSplashScreen();
                }
                else
                {
                    EditorUtility.DisplayDialog("Premium", "You need to be a premium user to use this feature.", "OK");
                }
            }
            catch (NullReferenceException)
            {

                EditorUtility.DisplayDialog("Premium", "You need to be logged in to use this feature.", "OK");
                NanoApiManager.OpenLoginWindow();
             
            }
        }

        [MenuItem("nanoSDK/Help/How to upload: Avatar", false, 1050)]
        public static void OpenAvatarLink()
        {
            Application.OpenURL("https://nanoSDK.net/tutorialAvatar");
        }
        
        [MenuItem("nanoSDK/Help/How to upload: World", false, 1051)]
        public static void OpenWorldLink()
        {
            Application.OpenURL("https://nanoSDK.net/tutorialWorld");
        }
        
        [MenuItem("nanoSDK/Help/How to update: SDK", false, 1052)]
        public static void OpenSDKUpdateLink()
        {
            Application.OpenURL("https://nanoSDK.net/updateSDK");
        }
        
        [MenuItem("nanoSDK/Help/How to update: Assets", false, 1053)]
        public static void OpenAssetUpdateLink()
        {
            Application.OpenURL("https://nanoSDK.net/updateAssets");
        }
        
        [MenuItem("nanoSDK/Help/Utilities/Update configs", false, 1000)]
        public static void ForceUpdateConfigs()
        {
            NanoSDK_ImportManager.UpdateConfig();
        }
        [MenuItem("nanoSDK/Help/Utilities/Check for Sdk Updates")]
        public static void UpdatesdkBtn()
        {
            NanoApiManager.CheckServerVersion("latest");
        }

    }
}