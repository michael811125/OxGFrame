using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;
using UnityEngine.UI;

public class HotfixTester : MonoBehaviour
{
    public Text printer;
    public GameObject hotfixComponent;

    private bool _isLoaded = false;

    private void Update()
    {
        if (!this._isLoaded)
        {
            this._isLoaded = true;

            string msg = ">>> Hotfix Tester <<<";
            this.printer.text = msg;

            // Hotfix use AddComponent
            var com = this.hotfixComponent.AddComponent<HotfixComponent>();
            msg = com.GetMessageFromHotfixComponent();
            this.printer.text += msg;

            // Hotfix use Load AssetBundle and Instantiate
            UniTask.Void(async () =>
            {
                var go = await AssetLoaders.InstantiateAssetAsync<GameObject>("HotfixPackage", "HotfixPrefab");
                var com = go.GetComponent<HotfixPrefab>();
                msg = com.GetMessageFromHotfixPrefab();
                this.printer.text += msg;
            });
        }
    }
}
