using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace nanoSDK
{
    [InitializeOnLoad]
    public class NanoSDK_StartApi
    {
        static NanoSDK_StartApi()
        {
            StartProgram();
        }

        [InitializeOnLoadMethod]
        public static void StartProgram()
        {
            StartLoginWindow();
        }

        private static void StartLoginWindow()
        {
            const string CONTENT_PO = "ProjectOpened";
            if (SessionState.GetBool(CONTENT_PO, false) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.SetBool(CONTENT_PO, true);
            EditorApplication.delayCall += NanoApiManager.OpenLoginWindow;
        }
    }

    public class NanoSDK_Login : EditorWindow
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

        //[MenuItem("nanoSDK/Login", false, 501)]


        public void OnEnable()
        {
            maxSize = new Vector2(600, 350);
            minSize = maxSize;
        }
        private void OnLostFocus()
        {
            if (NanoApiManager.IsLoggedInAndVerified()) return;
            EditorUtility.DisplayDialog("nanoAPI", "Please login and provide a license key to use nanoSDK", "Okay");
            Focus();
        }

        private void OnDestroy()
        {
            if (NanoApiManager.IsLoggedInAndVerified()) return;
            EditorApplication.delayCall += NanoApiManager.OpenLoginWindow;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (NanoApiManager.IsUserLoggedIn())
            {
                InitializeData();

                if (!NanoApiManager.IsLoggedInAndVerified())
                {
                    EditorGUILayout.LabelField("License Key");
                    redeemCode = EditorGUILayout.TextField("Key", redeemCode);
                    if (GUILayout.Button("Redeem"))
                    {
                        if (string.IsNullOrWhiteSpace(redeemCode))
                            EditorUtility.DisplayDialog("nanoSDK Api", "LicenseKey Cant be Empty!", "Okay");
                        else NanoApiManager.RedeemLicense(redeemCode);
                    }
                }

                if (GUILayout.Button("Logout")) NanoApiManager.Logout();
                if (GUILayout.Button("Copy Data for support"))
                {

                    string copyContent = $@"
Username: {NanoApiManager.User.Username}
Email: {NanoApiManager.User.Email}
ID: {NanoApiManager.User.ID}
                    ";
                    EditorGUIUtility.systemCopyBuffer = copyContent;
                }

                GUILayout.Space(4);
                
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField("Login");

            userInputText = EditorGUILayout.TextField("Username", userInputText);
            passInputText = EditorGUILayout.PasswordField("Password", passInputText);
            if (GUILayout.Button("Login"))
            {
                if (string.IsNullOrWhiteSpace(userInputText) || string.IsNullOrWhiteSpace(passInputText))
                    EditorUtility.DisplayDialog("nanoSDK Api", "Credentials Cant be Empty!", "Okay");
                else NanoApiManager.Login(userInputText, passInputText);
            }

            EditorGUILayout.LabelField("Register an Account");
            regUserInputText = EditorGUILayout.TextField("Username", regUserInputText);
            regPassInputText = EditorGUILayout.PasswordField("Password", regPassInputText);
            regEmailInputText = EditorGUILayout.TextField("Email", regEmailInputText);

            if (GUILayout.Button("Register"))
            {
                if (string.IsNullOrEmpty(regUserInputText) || string.IsNullOrEmpty(regPassInputText) ||
                    string.IsNullOrEmpty(regEmailInputText))
                    EditorUtility.DisplayDialog("nanoSDK Api", "Credentials Cant be Empty!", "Okay");
                else NanoApiManager.Register(regUserInputText, regPassInputText, regEmailInputText);
            }

            EditorGUILayout.EndVertical();
        }

        private void InitializeData()
        {
            EditorGUILayout.LabelField($"ID:  {NanoApiManager.User.ID}");
            EditorGUILayout.LabelField($"Logged in as:  {NanoApiManager.User.Username}");
            EditorGUILayout.LabelField($"Email:  {NanoApiManager.User.Email}");
            EditorGUILayout.LabelField($"Permission: {NanoApiManager.User.Permission}");
            EditorGUILayout.LabelField($"Verified:  {NanoApiManager.User.IsVerified}");
            EditorGUILayout.LabelField($"Premium:  {NanoApiManager.User.IsPremium}");
        }
    }
}