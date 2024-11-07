using Cysharp.Threading.Tasks;
using OxGFrame.MediaFrame.AudioFrame;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.MediaFrame.Editor
{
    [CustomEditor(typeof(AudioBase))]
    public class AudioBaseEditor : UnityEditor.Editor
    {
        protected AudioBase _target = null;

        // Properties
        protected SerializedProperty _audioLength;

        private void OnEnable()
        {
            this._target = (AudioBase)target;

            // Init Properties
            this._audioLength = serializedObject.FindProperty("audioLength");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Draw Views
            this.DrawAudioLengthView();
        }

        protected virtual async void DrawAudioLengthView()
        {
            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = _Make2DTexture(1, 1, new Color(0f, 0.5f, 0.25f, 0.5f));
            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = Color.cyan;

            serializedObject.Update();
            EditorGUILayout.PropertyField(this._audioLength);
            if (GUILayout.Button("Preload"))
            {
                // must set dirty (save will be success)
                EditorUtility.SetDirty(this._target);

                UniTask.Void(async () =>
                {
                    AudioClip audioClip = null;
                    switch (this._target.sourceType)
                    {
                        case SourceType.Audio:
                            audioClip = this._target.audioClip;
                            if (audioClip != null) this._audioLength.floatValue = this._target.audioLength = this._target.audioClip.length;
                            else Debug.LogError("Cannot find AudioClip");
                            break;

                        case SourceType.StreamingAssets:
                            audioClip = await this._target.GetAudioFromStreamingAssets(false);
                            if (audioClip != null) this._audioLength.floatValue = this._target.audioLength = audioClip.length;
                            break;

                        case SourceType.Url:
                            audioClip = await this._target.GetAudioFromURL(false);
                            if (audioClip != null) this._audioLength.floatValue = this._target.audioLength = audioClip.length;
                            break;
                    }

                    if (audioClip != null)
                        Debug.Log($"<color=#ffe700>AudioClip Info => Channel: {audioClip.channels}, Frequency: {audioClip.frequency}, Sample: {audioClip.samples}, Length: {audioClip.length}, State: {audioClip.loadState}</color>");
                    else
                        Debug.Log($"<color=#ff0000>AudioClip request failed!!!</color>");

                    serializedObject.ApplyModifiedProperties();
                });
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.EndHorizontal();
        }

        private Texture2D _Make2DTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}