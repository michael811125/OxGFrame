using OxGFrame.CoreFrame.Utility;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(ButtonPlus))]
public class ButtonPlusEditor : ButtonEditor
{
    SerializedProperty _onLongClickProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        this._onLongClickProperty = serializedObject.FindProperty("_onLongClick");
    }

    public override void OnInspectorGUI()
    {
        // 查找ButtonPlus目標
        ButtonPlus _target = (ButtonPlus)target;

        // 顯示長按區塊
        _target.isLongPress = EditorGUILayout.Toggle(new GUIContent("IsLongPress", "To enable Long Press"), _target.isLongPress);
        if (_target.isLongPress) this._ShowLongPress(_target);

        // 顯示ExtdTransition區塊
        this._ShowExtdTransition(_target, _target.extdTransition);

        // 顯示ButtonEditor區塊
        base.OnInspectorGUI();

        // 顯示LongClick事件區塊
        if (_target.isLongPress) this._ShowLongClickEvent();
    }

    private void _ShowLongPress(ButtonPlus target)
    {
        EditorGUI.indentLevel++; // 縮排變動
        target.holdTime = EditorGUILayout.FloatField(new GUIContent("HoldTime", "Set Long Press time to invoke event"), target.holdTime);
        target.cdTime = EditorGUILayout.FloatField(new GUIContent("CdTime", "Block when you Long Press continue time"), target.cdTime);
        EditorGUI.indentLevel--; // 復原縮排
    }

    private void _ShowLongClickEvent()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(this._onLongClickProperty);
        serializedObject.ApplyModifiedProperties();
    }

    private void _ShowExtdTransition(ButtonPlus target, ButtonPlus.ExtdTransition option)
    {
        target.extdTransition = (ButtonPlus.ExtdTransition)EditorGUILayout.EnumPopup("ExtdTransition", target.extdTransition);

        switch (option)
        {
            case ButtonPlus.ExtdTransition.None:
                break;

            case ButtonPlus.ExtdTransition.Scale:
                EditorGUI.indentLevel++; // 縮排變動
                target.transScale.size = EditorGUILayout.FloatField(new GUIContent("Size", "While click button will be set scale size"), target.transScale.size);
                EditorGUI.indentLevel--; // 復原縮排
                break;
        }
    }
}