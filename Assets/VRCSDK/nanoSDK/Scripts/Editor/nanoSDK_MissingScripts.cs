using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace nanoSDK
{ 
    public class NanoSDK_MissingScripts
    {
        [MenuItem("nanoSDK/DelteMissingScripts", false, 200)]
        public static void GetAndDelScripts()
        {
            var gmjs = GameObject.FindObjectsOfType<GameObject>().Where(gmj => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gmj) != 0).ToArray();
            for (int i = 0; i < gmjs.Length; i++)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gmjs[i]);
            }
            string message = gmjs.Length == 0 ? "No missing scripts found" : $"{gmjs.Length} missing scripts deleted";
            NanoLog(message);

        }

        private static void NanoLog(string message)
        {
            message = "<color=magenta>" + message + "</color>";
            Debug.Log("[nanoSDK_MissingScripts]: " + message);
            message = "<color=white>" + message + "</color>";
            
        }
    }
}
