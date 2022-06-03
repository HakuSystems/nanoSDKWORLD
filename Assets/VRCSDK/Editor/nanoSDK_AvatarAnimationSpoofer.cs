using nanoSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor;
using Newtonsoft.Json;
using System.Net;

namespace nanoSDK
{
    public class nanoSDK_AvatarAnimationSpoofer : EditorWindow
    {
        [MenuItem("nanoSDK/Generate Hashes", false, 800)]
        private static void OpenWindow()
        {
            GenerateHashes($"Assets{Path.DirectorySeparatorChar}VRCSDK{Path.DirectorySeparatorChar}nanoSDK");

        }

        public static string GenerateHashes(string path)
        {
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .OrderBy(p => p).ToList();
            File.WriteAllText($"{path}{Path.DirectorySeparatorChar}hashes.txt", "");
            foreach (var file in files)
            {
                if (!file.EndsWith(".cs"))
                {
                    continue;
                }
                var md5 = MD5.Create();
                var hash = md5.ComputeHash(File.ReadAllBytes(file));
                var result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                Debug.Log(file + " : " + result);
                
                File.AppendAllText($"{path}{Path.DirectorySeparatorChar}hashes.txt", $"{result}\n");
            }
            return "";

        }
    }
}
