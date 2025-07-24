using OxGFrame.AssetLoader.Bundle;

namespace OxGFrame.AssetLoader.Editor
{
    public static class CryptogramSettingSetup
    {
        private static CryptogramSetting _cryptogramSetting;

        public static CryptogramSetting GetCryptogramSetting()
        {
            if (_cryptogramSetting == null)
                _cryptogramSetting = EditorTool.LoadSettingData<CryptogramSetting>();
            return _cryptogramSetting;
        }
    }
}