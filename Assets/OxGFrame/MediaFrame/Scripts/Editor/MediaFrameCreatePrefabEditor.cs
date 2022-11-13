using OxGFrame.MediaFrame.AudioFrame;
using OxGFrame.MediaFrame.VideoFrame;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public static class MediaFrameCreatePrefabEditor
{
    class DoCreatePrefabAsset : EndNameEditAction
    {
        // Subclass and override this method to create specialised prefab asset creation functions
        protected virtual GameObject CreateGameObject(string name)
        {
            return new GameObject(name);
        }

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            GameObject go = CreateGameObject(Path.GetFileNameWithoutExtension(pathName));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, pathName);
            GameObject.DestroyImmediate(go);
        }
    }

    class DoCreateAudioSolePrefabAsset : DoCreatePrefabAsset
    {
        protected override GameObject CreateGameObject(string name)
        {
            var obj = new GameObject(name, typeof(AudioBase));

            // sole settings
            AudioBase audBase = obj.GetComponent<AudioBase>();
            audBase.audioType.soundType = SoundType.Sole;
            audBase.loops = -1;
            audBase.onStopAndDestroy = true;
            audBase.onDestroyAndUnload = true;
            audBase.autoEndToStop = false;

            return obj;
        }
    }

    class DoCreateAudioSoundEffectPrefabAsset : DoCreatePrefabAsset
    {
        protected override GameObject CreateGameObject(string name)
        {
            var obj = new GameObject(name, typeof(AudioBase));

            // sound effect settings
            AudioBase audBase = obj.GetComponent<AudioBase>();
            audBase.audioType.soundType = SoundType.SoundEffect;
            audBase.loops = 0;
            audBase.onStopAndDestroy = true;
            audBase.onDestroyAndUnload = false;
            audBase.autoEndToStop = true;

            return obj;
        }
    }

    class DoCreateVideoRenderTexturePrefabAsset : DoCreatePrefabAsset
    {
        protected override GameObject CreateGameObject(string name)
        {
            var obj = new GameObject(name, typeof(VideoBase));

            // render texture settings
            VideoBase vidBase = obj.GetComponent<VideoBase>();
            vidBase.renderMode = OxGFrame.MediaFrame.VideoFrame.RenderMode.RenderTexture;
            vidBase.onStopAndDestroy = true;
            vidBase.onDestroyAndUnload = true;
            vidBase.autoEndToStop = true;

            return obj;
        }
    }

    class DoCreateVideoCameraPrefabAsset : DoCreatePrefabAsset
    {
        protected override GameObject CreateGameObject(string name)
        {
            var obj = new GameObject(name, typeof(VideoBase));

            // camera settings
            VideoBase vidBase = obj.GetComponent<VideoBase>();
            vidBase.renderMode = OxGFrame.MediaFrame.VideoFrame.RenderMode.Camera;
            vidBase.onStopAndDestroy = true;
            vidBase.onDestroyAndUnload = true;
            vidBase.autoEndToStop = true;

            return obj;
        }
    }

    static void CreatePrefabAsset(string name, DoCreatePrefabAsset createAction)
    {
        string directory = GetSelectedAssetDirectory();
        string path = Path.Combine(directory, $"{name}.prefab");
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, createAction, path, EditorGUIUtility.FindTexture("Prefab Icon"), null);
    }

    static string GetSelectedAssetDirectory()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (Directory.Exists(path))
            return path;
        else
            return Path.GetDirectoryName(path);
    }

    [MenuItem("Assets/Create/OxGFrame/MediaFrame/Audio/TplPrefabs/TplAudioSole (Sole Audio Prefab)", isValidateFunction: false, priority: 51)]
    public static void CreateTplAudioSole()
    {
        CreatePrefabAsset("NewTplAudioSole", ScriptableObject.CreateInstance<DoCreateAudioSolePrefabAsset>());
    }

    [MenuItem("Assets/Create/OxGFrame/MediaFrame/Audio/TplPrefabs/TplAudioSoundEffect (SoundEffect Audio Prefab)", isValidateFunction: false, priority: 51)]
    public static void CreateTplAudioSoundEffect()
    {
        CreatePrefabAsset("NewTplAudioSoundEffect", ScriptableObject.CreateInstance<DoCreateAudioSoundEffectPrefabAsset>());
    }

    [MenuItem("Assets/Create/OxGFrame/MediaFrame/Video/TplPrefabs/TplVideoRenderTexture (RenderTexture Video Prefab)", isValidateFunction: false, priority: 51)]
    public static void CreateTplVideoRenderTexture()
    {
        CreatePrefabAsset("NewTplVideoRenderTexture", ScriptableObject.CreateInstance<DoCreateVideoRenderTexturePrefabAsset>());
    }

    [MenuItem("Assets/Create/OxGFrame/MediaFrame/Video/TplPrefabs/TplVideoCamera (Camera Video Prefab)", isValidateFunction: false, priority: 51)]
    public static void CreateTplVideoCamera()
    {
        CreatePrefabAsset("NewTplVideoCamera", ScriptableObject.CreateInstance<DoCreateVideoCameraPrefabAsset>());
    }
}