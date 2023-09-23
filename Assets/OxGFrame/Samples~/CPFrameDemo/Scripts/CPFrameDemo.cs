using OxGFrame.CoreFrame;
using UnityEngine;

public static class TplPrefs
{
    // if use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/Prefabs/";

    // Assets
    public static readonly string Demo1CP = $"{_prefix}{_path}Demo1CP";
    public static readonly string Demo2CP = $"{_prefix}{_path}Demo2CP";
}

public class CPFrameDemo : MonoBehaviour
{
    public Transform container;

    private void Awake()
    {
        // If Init instance can more efficiency
        CoreFrames.CPFrame.InitInstance();
    }

    public void LoadDemoPref1()
    {
        // Just clone CP without Factory Mode
        var pref = CoreFrames.CPFrame.LoadWithClone<Demo1CP>(TplPrefs.Demo1CP);
        if (pref != null) pref.MyMethod();
    }

    private int _price = 10;
    public async void LoadDemoPref2()
    {
        // Clone template CP with Factory Mode (can make ItemIcon, enemyModel, and so on.)
        var pref = await Demo2CP.CloneParsedDemo2CP(this._price, this.container);
        if (pref != null)
        {
            pref.PrintPrice();
            this._price++;
        }
    }
}
