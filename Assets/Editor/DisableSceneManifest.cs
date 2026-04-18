#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class DisableSceneManifest
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        // Ensure the scene manifest exists with UIApplicationSupportsMultipleScenes=false.
        // We need scenes so our DeepLinkHandler.mm's scene:openURLContexts: intercepts URLs
        // BEFORE the AppDelegate (which triggers Web3Auth's buggy handler).
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        var sceneManifest = plist.root["UIApplicationSceneManifest"]?.AsDict();
        if (sceneManifest != null)
        {
            sceneManifest.SetBoolean("UIApplicationSupportsMultipleScenes", false);
        }

        plist.WriteToFile(plistPath);
    }
}
#endif
