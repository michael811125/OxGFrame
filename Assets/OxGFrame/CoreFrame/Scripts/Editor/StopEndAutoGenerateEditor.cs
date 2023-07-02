using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    public static class StopEndAutoGenerateEditor
    {
        private static List<GameObject> _listGos = new List<GameObject>();

        #region MenuItem
        [MenuItem("GameObject/OxGFrame/Auto Generate Stop End Symbol (Shift+E) #e", false, 0)]
        public static void Execute()
        {
            if (Selection.gameObjects.Length == 0) return;

            // 檢查選擇物件是不是 Root 節點
            if (!_CheckIsRoot()) return;

            // 遍歷選擇的各 root 節點
            foreach (var root in Selection.gameObjects)
            {
                // 開始向下搜索
                _StartSearchDown(root);
                // 生成 StopEnd
                _GenerateStopEndSymbol();
            }
        }
        #endregion

        #region Check
        private static bool _CheckIsRoot()
        {
            foreach (var select in Selection.gameObjects)
            {
                if (select.GetInstanceID() != select.transform.root.gameObject.GetInstanceID())
                {
                    Debug.Log($"<color=#ff2486>Selected is not a root node => Selected: <color=#ffb824>{select.name}</color>, Root: <color=#ffec24>{select.transform.root.name}</color></color>", select.transform.root);
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Search
        private static void _StartSearchDown(GameObject root)
        {
            _listGos.Clear();
            _SearchDown(root);
        }

        private static void _SearchDown(GameObject go)
        {
            _listGos.Add(go);

            // 遞迴蒐集
            foreach (Transform child in go.GetComponentInChildren<Transform>())
            {
                _SearchDown(child.gameObject);
            }
        }
        #endregion

        #region Generate
        internal static void _GenerateStopEndSymbol()
        {
            if (_listGos.Count == 0) return;

            // 清除原有的 StopEnd
            foreach (var go in _listGos)
            {
                if (Binder.CheckNodeIsStopEnd(go.name))
                {
                    go.name = go.name.Replace(FrameConfig.BIND_STOP_END, string.Empty);
                }
            }

            // 由最後開始查找到第一個綁定物件後停止
            int lastIdx = -1;
            for (int i = _listGos.Count - 1; i >= 0; i--)
            {
                var lastGo = _listGos[i];
                if (Binder.CheckNodeHasPrefix(lastGo.name))
                {
                    lastIdx = i;
                    break;
                }
            }

            if (lastIdx != -1)
            {
                GameObject endNode = ((lastIdx + 1) == _listGos.Count) ? null : _listGos[lastIdx + 1];

                // 最後節點為綁定物件
                if (endNode == null)
                {
                    endNode = _listGos[lastIdx];
                    Debug.Log($"<color=#fff472>Last node is a bind object, doesn't need to generate stop end symbol: <color=#72e4ff>{endNode.name}</color></color>", endNode);
                    return;
                }

                // 最後綁定物件的下一個節點
                Undo.RecordObject(endNode, $"Modified Name with Stop End {endNode.name}");
                endNode.name = $"{endNode.name}{FrameConfig.BIND_STOP_END}";

                Debug.Log($"<color=#b5ff72>Auto generate stop end symbol ({FrameConfig.BIND_STOP_END}) => EndNode: <color=#72e4ff>{endNode.name}</color></color>", endNode);
            }
            else
            {
                // RectTransform (Prefab)
                if (_listGos.Count > 2 && _listGos[0].name.Contains("(Environment)"))
                {
                    GameObject firstChild = _listGos[2];
                    Undo.RecordObject(firstChild, $"Modified Name with Stop End {firstChild.name}");
                    firstChild.name = $"{firstChild.name}{FrameConfig.BIND_STOP_END}";

                    Debug.Log($"<color=#ff72b3>There are no any bind objects, generate stop end symbol to first child: <color=#72e4ff>{firstChild.name}</color></color>", firstChild);
                }
                // Transform (Prefab)
                else if (_listGos.Count > 1 && !_listGos[0].name.Contains("(Environment)"))
                {
                    GameObject firstChild = _listGos[1];
                    Undo.RecordObject(firstChild, $"Modified Name with Stop End {firstChild.name}");
                    firstChild.name = $"{firstChild.name}{FrameConfig.BIND_STOP_END}";

                    Debug.Log($"<color=#ff72b3>There are no any bind objects, generate stop end symbol to first child: <color=#72e4ff>{firstChild.name}</color></color>", firstChild);
                }
            }
        }
        #endregion
    }
}