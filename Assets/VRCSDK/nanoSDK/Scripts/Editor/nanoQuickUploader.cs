using nanoSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
//using VRC.Core;
//using VRC.SDKBase.Editor;

namespace Assets.VRCSDK.nanoSDK.Scripts.Editor
{
    public class NanoQuickUploader
    {
        [MenuItem("nanoSDK/QuickUpload", false, 560)]
        public static void QuickUploadContent()
        {
           nanoSDK_FastUploader.RunFastUpload();
        }
    }
}
