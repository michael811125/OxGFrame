using Cysharp.Threading.Tasks;
using OxGFrame.EventCenter;
using UnityEngine;

public class EventMsgTest : EventBase
{
    private int e_valueInt;
    private string e_valueString;

    public void Emit(int valueInt, string valueString)
    {
        this.e_valueInt = valueInt;
        this.e_valueString = valueString;

        this.HandleEvent().Forget();
    }

    public async override UniTaskVoid HandleEvent()
    {
        Debug.Log(string.Format("<color=#FFC078>【Handle Event】 -> {0}</color>", nameof(EventMsgTest)));

        int getValueInt = this.e_valueInt;
        string getValueString = this.e_valueString;

        Debug.Log($"Get Values: {getValueInt}, {getValueString}");

        this.Release();
    }

    protected override void Release()
    {
        this.e_valueInt = 0;
        this.e_valueString = null;
    }
}