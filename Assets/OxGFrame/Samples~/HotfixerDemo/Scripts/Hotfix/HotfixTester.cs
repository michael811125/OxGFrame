using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;
using UnityEngine.UI;

public class HotfixTester : MonoBehaviour
{
    public Text printer;
    public GameObject hotfixComponent;

    private bool _isLoaded = false;

    private async void Start()
    {
        // Check patch
        AssetPatcher.Check();
    }

    private void Update()
    {
        if (AssetPatcher.IsDone() && !this._isLoaded)
        {
            this._isLoaded = true;

            string msg = ">>> Hotfix Tester <<<";
            this.printer.text = msg;

            // Hotfix by AddComponent
            var com = this.hotfixComponent.AddComponent<HotfixComponent>();
            msg = com.GetMessageFromHotfixComponent();
            this.printer.text += msg;

            // Hotfix by Load AssetBundle and Instantiate
            UniTask.Void(async () =>
            {
                var go = await AssetLoaders.InstantiateAssetAsync<GameObject>("HotfixPrefab");
                var com = go.GetComponent<HotfixPrefab>();
                msg = com.GetMessageFromHotfixPrefab();
                this.printer.text += msg;
            });
        }
    }
}
