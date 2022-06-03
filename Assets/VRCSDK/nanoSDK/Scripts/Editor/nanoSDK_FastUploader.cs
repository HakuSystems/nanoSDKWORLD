using nanoSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using VRC.Core;
using VRC.SDKBase.Editor;

[ExecuteInEditMode]
public class nanoSDK_FastUploader
{
    public static void RunFastUpload()
    {
        //bool checkedForIssues = false;
        //if (!checkedForIssues)
        //  EnvConfig.ConfigurePlayerSettings();



List<VRC.SDKBase.VRC_AvatarDescriptor> allavatars = VRC.Tools.FindSceneObjectsOfTypeAll<VRC.SDKBase.VRC_AvatarDescriptor>().ToList();

var avatars = allavatars.Where(av => av.gameObject.activeInHierarchy).ToArray();
var avatar = allavatars[0];

if (APIUser.CurrentUser.canPublishAvatars)
{
    if (EditorUtility.DisplayDialog("nanoSDK EasyUpload", "Avatar: " + "[" + avatar.gameObject.name + "]" + " Will be Uploaded now.", "Nice!", "Information for Quest Users"))
    {
        UploadNow(avatar.gameObject);
    }
    else
    {
        if (EditorUtility.DisplayDialog("nanoSDK EasyUpload", "This Method lets " +
            "you upload anything. but some of the content will be blocked " +
            "ingame because of VRChat Networking!", "Upload now"))
        {
            UploadNow(avatar.gameObject);
        }
    }
}
else
{
    EditorUtility.DisplayDialog("nanoSDK EasyUpload", "Cant Upload Avatar", "Upload Anyways");
    UploadNow(avatar.gameObject);
}

}

private static void UploadNow(GameObject gameObject)
{
NanoSDK_MissingScripts.GetAndDelScripts();
VRC_SdkBuilder.shouldBuildUnityPackage = false;
VRC_SdkBuilder.ExportAndUploadAvatarBlueprint(gameObject);
}


}
