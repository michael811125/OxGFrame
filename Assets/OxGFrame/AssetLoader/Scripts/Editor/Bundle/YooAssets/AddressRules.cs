
using YooAsset.Editor;

[DisplayName("定位地址: 自定義名稱")]
public class AddressByCustomName : IAddressRule
{
    string IAddressRule.GetAssetAddress(AddressRuleData data)
    {
        return data.UserData;
    }
}