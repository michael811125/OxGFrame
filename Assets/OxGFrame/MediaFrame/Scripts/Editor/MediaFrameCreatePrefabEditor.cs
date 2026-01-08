using OxGFrame.MediaFrame.AudioFrame;
using OxGFrame.MediaFrame.VideoFrame;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace OxGFrame.MediaFrame.Editor
{
    public static class MediaFrameCreatePrefabEditor
    {
        private class DoCreatePrefabAsset : EndNameEditAction
        {
            // Subclass and override this method to create specialised prefab asset creation functions
            public virtual GameObject CreateGameObject(string name)
            {
                return new GameObject(name);
            }

            public virtual void OnPrefabCreated(GameObject prefab)
            {
                // Override this to perform actions after prefab is created
            }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                GameObject go = this.CreateGameObject(Path.GetFileNameWithoutExtension(pathName));
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, pathName);
                GameObject.DestroyImmediate(go);

                // Call post-creation hook
                this.OnPrefabCreated(prefab);

                // Save assets to ensure changes are persisted
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private class DoCreateAudioSolePrefabAsset : DoCreatePrefabAsset
        {
            public override GameObject CreateGameObject(string name)
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

        private class DoCreateAudioSoundEffectPrefabAsset : DoCreatePrefabAsset
        {
            public override GameObject CreateGameObject(string name)
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

        private class DoCreateVideoRenderTexturePrefabAsset : DoCreatePrefabAsset
        {
            public override GameObject CreateGameObject(string name)
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

        private class DoCreateVideoCameraPrefabAsset : DoCreatePrefabAsset
        {
            public override GameObject CreateGameObject(string name)
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

        private class DoCreateAudioSolePrefabFromClip : DoCreatePrefabAsset
        {
            public AudioClip audioClip;

            public override GameObject CreateGameObject(string name)
            {
                var obj = new GameObject(name, typeof(AudioBase));

                // sole settings
                AudioBase audBase = obj.GetComponent<AudioBase>();
                audBase.audioType.soundType = SoundType.Sole;
                audBase.loops = -1;
                audBase.onStopAndDestroy = true;
                audBase.onDestroyAndUnload = true;
                audBase.autoEndToStop = false;

                // 設置 AudioClip
                if (this.audioClip != null)
                {
                    audBase.sourceType = AudioFrame.SourceType.Audio;
                    audBase.audioClip = this.audioClip;
                }

                return obj;
            }

            public override void OnPrefabCreated(GameObject prefab)
            {
                // Preload audio length after prefab is created
                if (this.audioClip != null)
                {
                    AudioBase audBase = prefab.GetComponent<AudioBase>();
                    if (audBase != null)
                    {
                        audBase.audioLength = this.audioClip.length;
                        EditorUtility.SetDirty(prefab);
                        Debug.Log($"<color=#ffe700>AudioClip Info => Channel: {this.audioClip.channels}, Frequency: {this.audioClip.frequency}, Sample: {this.audioClip.samples}, Length: {this.audioClip.length}, State: {this.audioClip.loadState}, Preload Audio Data*: {this.audioClip.preloadAudioData}</color>");
                    }
                }
            }
        }

        private class DoCreateAudioSoundEffectPrefabFromClip : DoCreatePrefabAsset
        {
            public AudioClip audioClip;

            public override GameObject CreateGameObject(string name)
            {
                var obj = new GameObject(name, typeof(AudioBase));

                // sound effect settings
                AudioBase audBase = obj.GetComponent<AudioBase>();
                audBase.audioType.soundType = SoundType.SoundEffect;
                audBase.loops = 0;
                audBase.onStopAndDestroy = true;
                audBase.onDestroyAndUnload = false;
                audBase.autoEndToStop = true;

                // 設置 AudioClip
                if (this.audioClip != null)
                {
                    audBase.sourceType = AudioFrame.SourceType.Audio;
                    audBase.audioClip = this.audioClip;
                }

                return obj;
            }

            public override void OnPrefabCreated(GameObject prefab)
            {
                // Preload audio length after prefab is created
                if (this.audioClip != null)
                {
                    AudioBase audBase = prefab.GetComponent<AudioBase>();
                    if (audBase != null)
                    {
                        audBase.audioLength = this.audioClip.length;
                        EditorUtility.SetDirty(prefab);
                        Debug.Log($"<color=#ffe700>AudioClip Info => Channel: {this.audioClip.channels}, Frequency: {this.audioClip.frequency}, Sample: {this.audioClip.samples}, Length: {this.audioClip.length}, State: {this.audioClip.loadState}, Preload Audio Data*: {this.audioClip.preloadAudioData}</color>");
                    }
                }
            }
        }

        private class DoCreateVideoRenderTexturePrefabFromClip : DoCreatePrefabAsset
        {
            public UnityEngine.Video.VideoClip videoClip;

            public override GameObject CreateGameObject(string name)
            {
                var obj = new GameObject(name, typeof(VideoBase));

                // render texture settings
                VideoBase vidBase = obj.GetComponent<VideoBase>();
                vidBase.renderMode = OxGFrame.MediaFrame.VideoFrame.RenderMode.RenderTexture;
                vidBase.onStopAndDestroy = true;
                vidBase.onDestroyAndUnload = true;
                vidBase.autoEndToStop = true;

                // 設置 VideoClip
                if (this.videoClip != null)
                {
                    vidBase.sourceType = VideoFrame.SourceType.Video;
                    vidBase.videoClip = this.videoClip;
                }

                return obj;
            }
        }

        private class DoCreateVideoCameraPrefabFromClip : DoCreatePrefabAsset
        {
            public UnityEngine.Video.VideoClip videoClip;

            public override GameObject CreateGameObject(string name)
            {
                var obj = new GameObject(name, typeof(VideoBase));

                // camera settings
                VideoBase vidBase = obj.GetComponent<VideoBase>();
                vidBase.renderMode = OxGFrame.MediaFrame.VideoFrame.RenderMode.Camera;
                vidBase.onStopAndDestroy = true;
                vidBase.onDestroyAndUnload = true;
                vidBase.autoEndToStop = true;

                // 設置 VideoClip
                if (this.videoClip != null)
                {
                    vidBase.sourceType = VideoFrame.SourceType.Video;
                    vidBase.videoClip = this.videoClip;
                }

                return obj;
            }
        }

        private static void _CreatePrefabAsset(string name, DoCreatePrefabAsset createAction)
        {
            string directory = _GetSelectedAssetDirectory();
            string path = Path.Combine(directory, $"{name}.prefab");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, createAction, path, EditorGUIUtility.FindTexture("Prefab Icon"), null);
        }

        private static void _CreatePrefabAssetDirectly(string name, string directory, DoCreatePrefabAsset createAction)
        {
            string path = Path.Combine(directory, $"{name}.prefab");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            GameObject go = createAction.CreateGameObject(Path.GetFileNameWithoutExtension(path));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);

            // Call post-creation hook
            createAction.OnPrefabCreated(prefab);
        }

        private static string _GetSelectedAssetDirectory()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(path))
                return path;
            else
                return Path.GetDirectoryName(path);
        }

        #region Template Prefab 創建菜單
        [MenuItem("Assets/Create/OxGFrame/Media Frame/Audio/Template Prefabs/Template Audio Sole (Sole Audio Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateTplAudioSole()
        {
            _CreatePrefabAsset("NewTplAudioSole", ScriptableObject.CreateInstance<DoCreateAudioSolePrefabAsset>());
        }

        [MenuItem("Assets/Create/OxGFrame/Media Frame/Audio/Template Prefabs/Template Audio SoundEffect (SoundEffect Audio Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateTplAudioSoundEffect()
        {
            _CreatePrefabAsset("NewTplAudioSoundEffect", ScriptableObject.CreateInstance<DoCreateAudioSoundEffectPrefabAsset>());
        }

        [MenuItem("Assets/Create/OxGFrame/Media Frame/Video/Template Prefabs/Template Video RenderTexture (RenderTexture Video Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateTplVideoRenderTexture()
        {
            _CreatePrefabAsset("NewTplVideoRenderTexture", ScriptableObject.CreateInstance<DoCreateVideoRenderTexturePrefabAsset>());
        }

        [MenuItem("Assets/Create/OxGFrame/Media Frame/Video/Template Prefabs/Template Video Camera (Camera Video Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateTplVideoCamera()
        {
            _CreatePrefabAsset("NewTplVideoCamera", ScriptableObject.CreateInstance<DoCreateVideoCameraPrefabAsset>());
        }
        #endregion

        #region 從 Clip 轉換為 Prefab 的菜單
        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Audio Sole (Sole Audio Prefab)", isValidateFunction: false, priority: 30)]
        public static void ConvertToAudioSole()
        {
            var clips = Selection.GetFiltered<AudioClip>(SelectionMode.Assets);
            if (clips.Length == 0) return;

            string directory = _GetSelectedAssetDirectory();

            foreach (var clip in clips)
            {
                var createAction = ScriptableObject.CreateInstance<DoCreateAudioSolePrefabFromClip>();
                createAction.audioClip = clip;
                _CreatePrefabAssetDirectly(clip.name, directory, createAction);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00ff00>Successfully converted {clips.Length} AudioClip(s) to Audio Sole Prefab(s)!</color>");
        }

        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Audio Sole (Sole Audio Prefab)", isValidateFunction: true)]
        public static bool ValidateConvertToAudioSole()
        {
            return Selection.GetFiltered<AudioClip>(SelectionMode.Assets).Length > 0;
        }

        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Audio SoundEffect (SoundEffect Audio Prefab)", isValidateFunction: false, priority: 30)]
        public static void ConvertToAudioSoundEffect()
        {
            var clips = Selection.GetFiltered<AudioClip>(SelectionMode.Assets);
            if (clips.Length == 0) return;

            string directory = _GetSelectedAssetDirectory();

            foreach (var clip in clips)
            {
                var createAction = ScriptableObject.CreateInstance<DoCreateAudioSoundEffectPrefabFromClip>();
                createAction.audioClip = clip;
                _CreatePrefabAssetDirectly(clip.name, directory, createAction);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00ff00>Successfully converted {clips.Length} AudioClip(s) to Audio SoundEffect Prefab(s)!</color>");
        }

        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Audio SoundEffect (SoundEffect Audio Prefab)", isValidateFunction: true)]
        public static bool ValidateConvertToAudioSoundEffect()
        {
            return Selection.GetFiltered<AudioClip>(SelectionMode.Assets).Length > 0;
        }

        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Video RenderTexture (RenderTexture Video Prefab)", isValidateFunction: false, priority: 30)]
        public static void ConvertToVideoRenderTexture()
        {
            var clips = Selection.GetFiltered<UnityEngine.Video.VideoClip>(SelectionMode.Assets);
            if (clips.Length == 0) return;

            string directory = _GetSelectedAssetDirectory();

            foreach (var clip in clips)
            {
                var createAction = ScriptableObject.CreateInstance<DoCreateVideoRenderTexturePrefabFromClip>();
                createAction.videoClip = clip;
                _CreatePrefabAssetDirectly(clip.name, directory, createAction);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00ff00>Successfully converted {clips.Length} VideoClip(s) to Video RenderTexture Prefab(s)!</color>");
        }

        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Video RenderTexture (RenderTexture Video Prefab)", isValidateFunction: true)]
        public static bool ValidateConvertToVideoRenderTexture()
        {
            return Selection.GetFiltered<UnityEngine.Video.VideoClip>(SelectionMode.Assets).Length > 0;
        }

        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Video Camera (Camera Video Prefab)", isValidateFunction: false, priority: 30)]
        public static void ConvertToVideoCamera()
        {
            var clips = Selection.GetFiltered<UnityEngine.Video.VideoClip>(SelectionMode.Assets);
            if (clips.Length == 0) return;

            string directory = _GetSelectedAssetDirectory();

            foreach (var clip in clips)
            {
                var createAction = ScriptableObject.CreateInstance<DoCreateVideoCameraPrefabFromClip>();
                createAction.videoClip = clip;
                _CreatePrefabAssetDirectly(clip.name, directory, createAction);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00ff00>Successfully converted {clips.Length} VideoClip(s) to Video Camera Prefab(s)!</color>");
        }

        [MenuItem("Assets/OxGFrame/Media Frame/Convert/Convert to Video Camera (Camera Video Prefab)", isValidateFunction: true)]
        public static bool ValidateConvertToVideoCamera()
        {
            return Selection.GetFiltered<UnityEngine.Video.VideoClip>(SelectionMode.Assets).Length > 0;
        }
        #endregion
    }
}