using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(ButtonPlus))]
public class ButtonPlusEditor : ButtonEditor
{
    SerializedProperty _onLongClickProperty;
    private ButtonPlus _target = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        this._onLongClickProperty = serializedObject.FindProperty("_onLongClick");
    }

    public override void OnInspectorGUI()
    {
        // 查找 ButtonPlus 目標
        this._target = (ButtonPlus)target;

        // 顯示長按區塊
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        this._target.isLongPress = EditorGUILayout.Toggle(new GUIContent("IsLongPress", "To enable Long Press"), this._target.isLongPress);
        if (this._target.isLongPress) this._ShowLongPress(this._target);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }

        // 顯示 ExtdTransition 區塊
        this._ShowExtdTransition(this._target, this._target.extdTransition);

        // 顯示 ButtonEditor 區塊
        base.OnInspectorGUI();

        // 顯示 LongClick 事件區塊
        if (this._target.isLongPress) this._ShowLongClickEvent(this._target);
    }

    private void _ShowLongPress(ButtonPlus target)
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

        EditorGUI.indentLevel++; // 縮排變動
        target.holdTime = EditorGUILayout.FloatField(new GUIContent("HoldTime", "Set Long Press time to invoke event"), target.holdTime);
        target.cdTime = EditorGUILayout.FloatField(new GUIContent("CdTime", "Block when you Long Press continue time"), target.cdTime);
        EditorGUI.indentLevel--; // 復原縮排

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void _ShowLongClickEvent(ButtonPlus target)
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

        EditorGUILayout.PropertyField(this._onLongClickProperty);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void _ShowExtdTransition(ButtonPlus target, ButtonPlus.ExtdTransition option)
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

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

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
