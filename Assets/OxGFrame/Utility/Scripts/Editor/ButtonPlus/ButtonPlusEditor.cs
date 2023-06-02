using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace OxGFrame.Utility.Btn.Editor
{
    [CustomEditor(typeof(ButtonPlus))]
    public class ButtonPlusEditor : ButtonEditor
    {
        SerializedProperty _onLongClickPressedProperty;
        SerializedProperty _onLongClickReleasedProperty;
        private ButtonPlus _target = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            this._onLongClickPressedProperty = serializedObject.FindProperty("_onLongClickPressed");
            this._onLongClickReleasedProperty = serializedObject.FindProperty("_onLongClickReleased");
        }

        public override void OnInspectorGUI()
        {
            // set ButtonPlus target
            this._target = (ButtonPlus)target;

            // draw LongClick setting
            this.ShowExtdLongClick(this._target, this._target.extdLongClick);

            // draw ExtdTransition
            this.ShowExtdTransition(this._target, this._target.extdTransition);

            // draw ButtonEditor
            base.OnInspectorGUI();

            // draw LongClick event
            if (this._target.extdLongClick != ButtonPlus.ExtdLongClick.None) this.ShowLongClickEvent(this._target, this._target.extdLongClick);
        }

        protected void ShowExtdLongClick(ButtonPlus target, ButtonPlus.ExtdLongClick option)
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            target.extdLongClick = (ButtonPlus.ExtdLongClick)EditorGUILayout.EnumPopup("Extd Long Click", target.extdLongClick);

            switch (option)
            {
                case ButtonPlus.ExtdLongClick.None:
                    break;

                case ButtonPlus.ExtdLongClick.Once:
                    EditorGUI.indentLevel++;
                    target.triggerTime = EditorGUILayout.FloatField(new GUIContent("Trigger Time", "Set long click time to invoke event"), target.triggerTime);
                    EditorGUI.indentLevel--;
                    break;
                case ButtonPlus.ExtdLongClick.Continuous:
                    EditorGUI.indentLevel++;
                    target.triggerTime = EditorGUILayout.FloatField(new GUIContent("Trigger Time", "Set long click time to invoke event"), target.triggerTime);
                    target.intervalTime = EditorGUILayout.FloatField(new GUIContent("Interval Time", "Set hold continuous interval time"), target.intervalTime);
                    EditorGUI.indentLevel--;
                    break;
                case ButtonPlus.ExtdLongClick.PressedAndReleased:
                    EditorGUI.indentLevel++;
                    target.triggerTime = EditorGUILayout.FloatField(new GUIContent("Trigger Time", "Set long click time to invoke event"), target.triggerTime);
                    EditorGUI.indentLevel--;
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected void ShowLongClickEvent(ButtonPlus target, ButtonPlus.ExtdLongClick option)
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            switch (option)
            {
                case ButtonPlus.ExtdLongClick.Once:
                case ButtonPlus.ExtdLongClick.Continuous:
                    EditorGUILayout.PropertyField(this._onLongClickPressedProperty);
                    break;
                case ButtonPlus.ExtdLongClick.PressedAndReleased:
                    EditorGUILayout.PropertyField(this._onLongClickPressedProperty);
                    EditorGUILayout.PropertyField(this._onLongClickReleasedProperty);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected void ShowExtdTransition(ButtonPlus target, ButtonPlus.ExtdTransition option)
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            target.extdTransition = (ButtonPlus.ExtdTransition)EditorGUILayout.EnumPopup("Extd Transition", target.extdTransition);

            switch (option)
            {
                case ButtonPlus.ExtdTransition.None:
                    break;

                case ButtonPlus.ExtdTransition.Scale:
                    EditorGUI.indentLevel++;
                    target.transScale.size = EditorGUILayout.FloatField(new GUIContent("Size", "While click button will set scale size"), target.transScale.size);
                    EditorGUI.indentLevel--;
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}