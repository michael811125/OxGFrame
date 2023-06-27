using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    public class BindCodeClipboardWindow : EditorWindow
    {
        private static BindCodeClipboardWindow _instance = null;
        internal static BindCodeClipboardWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<BindCodeClipboardWindow>();
            return _instance;
        }

        private string _codes;
        private Vector2 _scrollview;
        private static Vector2 _windowSize = new Vector2(400f, 400f);

        public static void ShowWindow(string codes)
        {
            _instance = null;
            GetInstance().titleContent = new GUIContent("Bind Code Clipboard");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
            GetInstance()._codes = codes;
        }

        private void OnGUI()
        {
            this._DrawCodesView();
        }

        private void _DrawCodesView()
        {
            this._scrollview = EditorGUILayout.BeginScrollView(this._scrollview, true, true);

            EditorGUILayout.TextArea(this._codes, GUILayout.Height(position.height));

            EditorGUILayout.EndScrollView();
        }
    }
}