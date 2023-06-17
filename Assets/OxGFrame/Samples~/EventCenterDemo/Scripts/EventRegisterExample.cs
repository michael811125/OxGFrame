using Cysharp.Threading.Tasks;
using OxGFrame.AgencyCenter.EventCenter;
using UnityEngine;

public class EventMsgTest : EventBase
{
    private int valueInt;
    private string valueString;

    public void Emit(int valueInt, string valueString)
    {
        this.valueInt = valueInt;
        this.valueString = valueString;

        this.HandleEvent().Forget();
    }

    public async override UniTaskVoid HandleEvent()
    {
        Debug.Log(string.Format("<color=#FFC078>【Handle Event】 -> {0}</color>", nameof(EventMsgTest)));

        int getValueInt = this.valueInt;
        string getValueString = this.valueString;

        Debug.Log($"Get Values: {getValueInt}, {getValueString}");

        this.Release();
    }

    protected override void Release()
    {
        this.valueInt = 0;
        this.valueString = null;
    }
}