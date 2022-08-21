using Cysharp.Threading.Tasks;
using OxGFrame.EventCenter;
using UnityEngine;

public class EEventTest : EventBase
{
    private int e_valueInt;
    private string e_valueString;

    public EEventTest(int funcId) : base(funcId) { }

    public void Emit(int valueInt, string valueString)
    {
        this.e_valueInt = valueInt;
        this.e_valueString = valueString;

        this.HandleEvent().Forget();
    }

    public async override UniTaskVoid HandleEvent()
    {
        Debug.Log(string.Format("<color=#FFC078>【Handle Event】 -> FuncId: 0x{0}</color>", this.GetFuncId().ToString("X")));

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