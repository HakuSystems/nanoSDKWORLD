using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace nanoSDK
{
    public static class NanoApiManager
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        public static NanoUserData User;
        public static string SERVERVERSION;
        public static string SERVERURL;


        private const string BASE_URL = "https://api.nanosdk.net";
        private static readonly Uri UserSelfUri = new Uri(BASE_URL + "/user/self");
        private static readonly Uri SdkVersionUri = new Uri(BASE_URL + "/public/sdk/version");
        private static readonly Uri RedeemUri = new Uri(BASE_URL + "/user/redeemables/redeem");
        private static readonly Uri LoginUri = new Uri(BASE_URL + "/user/login");
        private static readonly Uri SignupUri = new Uri(BASE_URL + "/user/signup");
        private static bool running = false;
        private const string AppJson = "application/json";

        public static bool IsLoggedInAndVerified() => IsUserLoggedIn() && User.IsVerified;
        
        public static bool IsUserLoggedIn()
        {
            if (User == null && !string.IsNullOrWhiteSpace(NanoApiConfig.Config.AuthKey) && !running) CheckUserSelf();
            return User != null;
        }

        public static void OpenLoginWindow() => EditorWindow.GetWindow<NanoSDK_Login>(false,"Account");
                                                            
                                                

        private static void ClearLogin()
        {
            Log("Clearing login data");
            User = null;
            NanoApiConfig.Config.AuthKey = null;
            NanoApiConfig.Save();
            OpenLoginWindow();
        }

        private static async Task<HttpResponseMessage> MakeApiCall(HttpRequestMessage request)
        {
            if (!string.IsNullOrWhiteSpace(NanoApiConfig.Config.AuthKey))
            {
                request.Headers.Add("Auth-Key", NanoApiConfig.Config.AuthKey);
            }
            

            var response = await HttpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Log("Got 401 from api, please reauthenticate.");
                ClearLogin();
                throw new Exception("API Call could not be completed.");
            }

            return response;
        }

        private static async void CheckUserSelf()
        {
            running = true;
            Log("Checking user");
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = UserSelfUri
            };

            var response = await MakeApiCall(request);

            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<BaseResponse<NanoUserData>>(result);
            User = properties.Data;
            Log("Successfully checked user");
            running = false;
        }
        public static async void RedeemLicense(string code)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new LicenseData
            {
                Key = code
            }), Encoding.UTF8, AppJson);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = RedeemUri,
                Content = content
            };
            var response = await MakeApiCall(request);
            var props = JsonConvert.DeserializeObject<BaseResponse<object>>(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode)
            {
                EditorUtility.DisplayDialog("nanoAPI", "Redeem was successful!", "Okay");
                CheckUserSelf();
            }
            else
            {
                EditorUtility.DisplayDialog("nanoAPI", props.Message, "Okay");
            }
        }
        public static async void CheckServerVersion(string version)
        {
            //finden fix ding
            string currentVersion = File.ReadAllText("Assets/VRCSDK/version.txt");
            Log("Getting Versions");
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = SdkVersionUri
            };

            var response = await MakeApiCall(request);
            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<SdkVersionOutput<SdkVersionData>>(result);

            if (!response.IsSuccessStatusCode)
            {
                Log("Something Went Wrong");
                return;
            }
            if (currentVersion != properties.Data.Version.ToString() + "," + properties.Data.Type.ToString() + "," + properties.Data.Branch.ToString())
            {
                NanoSDK_AutomaticUpdateAndInstall.CheckServerVersionINTERN();
            }
            else
            {
                EditorUtility.DisplayDialog("You are up to date",
                    "Current nanoSDK version: V" + currentVersion,
                    "Okay"
                    );
            }
            Log(properties.Data.Branch.ToString()+ " - " +properties.Data.Version.ToString()+" - "+properties.Data.Url);
        }

        public static async void Login(string username, string password)
        {
            Log("Trying to login");
            var content = new StringContent(JsonConvert.SerializeObject(new APILoginData
            {
                Username = username,
                Password = password
            }), Encoding.UTF8, AppJson);
            var request = new HttpRequestMessage
            {
                RequestUri = LoginUri,
                Content = content,
                Method = HttpMethod.Post
            };

            var response = await MakeApiCall(request);
            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<BaseResponse<LoginResponse>>(result);

            if (!response.IsSuccessStatusCode)
            {
                Log("Login failed");
                ClearLogin();
                return;
            }

            NanoApiConfig.Config.AuthKey = properties.Data.AuthKey;
            NanoApiConfig.Save();
            Log("Successfully logged in");
            CheckUserSelf();
        }

        public static void Logout() => ClearLogin();

        public static async void Register(string username, string password, string email)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new APIRegisterData
            {
                Username = username,
                Password = password,
                Email = email
            }), Encoding.UTF8, AppJson);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = SignupUri,
                Content = content
            };

            var response = await MakeApiCall(request);
            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<BaseResponse<SanityCheckResponse>>(result);

            if (response.IsSuccessStatusCode)
            {
                EditorUtility.DisplayDialog("nanoSDK Api",
                    "Successfully Registered, autoLogin is being processed when pressed Okay", "Okay");
                Login(username, password);
                return;
            }

            if (properties.Message.Contains("Sanity checks"))
            {
                var sb = new StringBuilder();
                string usernameArray = null;
                string passwordArray = null;
                string emailArray = null;
                foreach (var item in properties.Data.UsernameSanityCheck)
                {
                    sb.AppendLine(item.Value);
                    usernameArray = sb.ToString();
                }

                sb.Clear();
                foreach (var item in properties.Data.PasswordSanityCheck)
                {
                    sb.AppendLine(item.Value);
                    passwordArray = sb.ToString();
                }

                sb.Clear();
                foreach (var item in properties.Data.EmailSanityCheck)
                {
                    sb.AppendLine(item.Value);
                    emailArray = sb.ToString();
                }

                sb.Clear();
                if (string.IsNullOrEmpty(usernameArray))
                {
                    usernameArray = "Valid";
                }

                if (string.IsNullOrEmpty(passwordArray))
                {
                    passwordArray = "Valid";
                }

                if (string.IsNullOrEmpty(emailArray))
                {
                    emailArray = "Valid";
                }

                if (EditorUtility.DisplayDialog("nanoSDK Api",
                    "Username: " + Environment.NewLine + usernameArray + Environment.NewLine + "Password: " +
                    Environment.NewLine + passwordArray + Environment.NewLine + "Email: " +
                    Environment.NewLine + emailArray, "Back"))
                {
                    OpenLoginWindow();
                }
            }
        }

        private static void Log(string msg)
        {

            msg = "<color=magenta>" + msg + "</color>";

            Debug.Log("[nanoAPI] " + msg);
            msg = "<color=white>" + msg + "</color>";
        }
    }
}