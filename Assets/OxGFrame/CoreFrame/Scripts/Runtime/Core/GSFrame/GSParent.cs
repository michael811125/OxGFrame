using UnityEngine;

namespace OxGFrame.CoreFrame.GSFrame
{
    public class GSParent : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (Time.frameCount == 0 || !Application.isPlaying) return;

            try
            {
                // 如果 Parent Destroy 時, 需要進行連動釋放, 確保 Manager 快取操作正常
                GSBase[] gsBases = this.gameObject.GetComponentsInChildren<GSBase>(true);
                foreach (var gsBase in gsBases)
                {
                    GSManager.GetInstance().Close(gsBase.assetName, true, true);
                }
            }
            catch
            {
                /* Nothing to do */
            }
        }
    }
}
