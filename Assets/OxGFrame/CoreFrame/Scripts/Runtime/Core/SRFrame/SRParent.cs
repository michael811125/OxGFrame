using UnityEngine;

namespace OxGFrame.CoreFrame.SRFrame
{
    public class SRParent : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (Time.frameCount == 0 || !Application.isPlaying)
                return;

            try
            {
                // 如果 Parent Destroy 時, 需要進行連動釋放, 確保 Manager 緩存操作正常
                SRBase[] srBases = this.gameObject.GetComponentsInChildren<SRBase>(true);
                foreach (var srBase in srBases)
                {
                    SRManager.GetInstance().Close(srBase.assetName, true, true);
                }
            }
            catch
            {
                /* Nothing to do */
            }
        }
    }
}
