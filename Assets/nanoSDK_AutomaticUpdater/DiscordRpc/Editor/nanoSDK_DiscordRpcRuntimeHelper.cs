using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace nanoSDK
{
    [InitializeOnLoadAttribute]
    public static class NanoSDK_DiscordRpcRuntimeHelper
    {
        // register an event handler when the class is initialized
        static NanoSDK_DiscordRpcRuntimeHelper()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
            EditorSceneManager.activeSceneChanged += SceneChanged;
        }

        private static void SceneChanged(Scene old, Scene next)
        {
            NanoSDK_DiscordRPC.SceneChanged(next);
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.EnteredEditMode)
            {
                NanoSDK_DiscordRPC.UpdateState(RpcState.EDITMODE);
                NanoSDK_DiscordRPC.ResetTime();
            } else if(state == PlayModeStateChange.EnteredPlayMode)
            {
                NanoSDK_DiscordRPC.UpdateState(RpcState.PLAYMODE);
                NanoSDK_DiscordRPC.ResetTime();
            }
        }
    }
}