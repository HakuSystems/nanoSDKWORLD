﻿using UnityEditor;
using UnityEngine;
using System.IO;

namespace nanoSDK.ThumbnailSelector
{
    [CustomEditor(typeof(nanoSDK_ThumbnailOverlay))]
    public class VRCThumbnailSelectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Image"))
            {
                GameObject obj = GameObject.Find("VRCCam");
                if (null == obj)
                {
                    Debug.Log("VRCCam not Found");
                    return;
                }

                nanoSDK_ThumbnailOverlay script = obj.GetComponent<nanoSDK_ThumbnailOverlay>();
                if (null == script)
                {
                    Debug.Log("nanoSDK_ThumbnailOverlay Script not Found");
                    return;
                }

                string path = EditorUtility.OpenFilePanel("Nigga Select Image", "", "png,jpg,jpeg");

                if (path.Length > 0)
                {
                    Texture2D texture = new Texture2D(1, 1);
                    if (null != texture)
                    {
                        texture.LoadImage(File.ReadAllBytes(path));
                        texture.filterMode = FilterMode.Point;
                        script.SetTextuer(texture);
                        script.enabled = true;
                    }
                }
            }
        }
    }
}