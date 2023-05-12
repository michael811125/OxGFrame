using UnityEngine;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIParent : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (Time.frameCount == 0 || !Application.isPlaying) return;

            try
            {
                // 如果 Parent Destroy 時, 需要進行連動釋放, 確保 Manager 緩存操作正常
                UIBase[] uiBases = this.gameObject.GetComponentsInChildren<UIBase>(true);
                foreach (var uiBase in uiBases)
                {
                    UIManager.GetInstance().Close(uiBase.assetName, true, true);
                }
            }
            catch
            {
                /* Nothing to do */
            }
        }
    }
}
