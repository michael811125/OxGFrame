using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    /// <summary>
    /// ComponentInfo 繪製器 (usingNamespace 欄位提供 Find 按鈕，可依組件名稱查找並選擇 using namespace)
    /// </summary>
    [CustomPropertyDrawer(typeof(CodeBindingSettings.ComponentInfo))]
    public class CodeBindingComponentInfoDrawer : PropertyDrawer
    {
        /// <summary>
        /// Find 按鈕寬度
        /// </summary>
        private const float _FIND_BUTTON_WIDTH = 42f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var componentNameProp = property.FindPropertyRelative("componentName");
            var usingNamespaceProp = property.FindPropertyRelative("usingNamespace");
            var pluralizeProp = property.FindPropertyRelative("pluralize");

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect rect = new Rect(position.x, position.y, position.width, line);

            // 折疊標頭 (維持與預設繪製一致的展開行為)
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Component Name
                rect.y += line + spacing;
                EditorGUI.PropertyField(rect, componentNameProp);

                // Using Namespace + Find 按鈕
                rect.y += line + spacing;
                Rect fieldRect = new Rect(rect.x, rect.y, rect.width - _FIND_BUTTON_WIDTH - 2f, line);
                Rect buttonRect = new Rect(rect.x + rect.width - _FIND_BUTTON_WIDTH, rect.y, _FIND_BUTTON_WIDTH, line);
                EditorGUI.PropertyField(fieldRect, usingNamespaceProp);
                if (GUI.Button(buttonRect, new GUIContent("Find", "Try to find the using namespace by component name.\n\nIf there are multiple candidates, a menu will pop up for selection."), EditorStyles.miniButton))
                {
                    _TryFindUsingNamespace(componentNameProp.stringValue, usingNamespaceProp);
                }

                // Pluralize (含子欄位)
                rect.y += line + spacing;
                EditorGUI.PropertyField(rect, pluralizeProp, true);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return line;

            var pluralizeProp = property.FindPropertyRelative("pluralize");
            return line + spacing +
                   line + spacing +
                   line + spacing +
                   EditorGUI.GetPropertyHeight(pluralizeProp, true);
        }

        /// <summary>
        /// 依組件名稱查找 using namespace (單一候選直接帶入，多候選彈出選單供選擇)
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="usingNamespaceProp"></param>
        private static void _TryFindUsingNamespace(string componentName, SerializedProperty usingNamespaceProp)
        {
            var candidates = UsingNamespaceFinder.FindCandidates(componentName);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[{nameof(CodeBindingSettings)}] Cannot find any type named: {componentName}. Please check the component name or install the related package.");
                return;
            }

            if (candidates.Count == 1)
            {
                _ApplyUsingNamespace(usingNamespaceProp.serializedObject, usingNamespaceProp.propertyPath, candidates[0].ns);
                Debug.Log($"<color=#00ffd1>[{nameof(CodeBindingSettings)}] Found using namespace => {componentName}: {UsingNamespaceFinder.GetDisplayName(candidates[0].ns)} ({candidates[0].assemblyName})</color>");
                return;
            }

            // 多候選彈出選單 (自行選擇正確的 using namespace)
            var menu = new GenericMenu();
            var serializedObject = usingNamespaceProp.serializedObject;
            string propertyPath = usingNamespaceProp.propertyPath;
            foreach (var candidate in candidates)
            {
                string ns = candidate.ns;
                string display = $"{UsingNamespaceFinder.GetDisplayName(ns)} ({candidate.assemblyName})";
                menu.AddItem(new GUIContent(display), false, () =>
                {
                    // GenericMenu 回呼於 GUI 流程之外，需以 propertyPath 重新取得屬性再套用
                    _ApplyUsingNamespace(serializedObject, propertyPath, ns);
                    Debug.Log($"<color=#00ffd1>[{nameof(CodeBindingSettings)}] Selected using namespace => {componentName}: {UsingNamespaceFinder.GetDisplayName(ns)}</color>");
                });
            }
            menu.ShowAsContext();
        }

        /// <summary>
        /// 套用 using namespace 至序列化屬性
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="propertyPath"></param>
        /// <param name="ns"></param>
        private static void _ApplyUsingNamespace(SerializedObject serializedObject, string propertyPath, string ns)
        {
            serializedObject.Update();
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
                return;
            property.stringValue = ns;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
