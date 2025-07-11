namespace OxGFrame.AssetLoader.Editor
{
    public static class CryptogramSettingSetup
    {
        public static CryptogramSetting cryptogramSetting;

        public static CryptogramSetting GetCryptogramSetting()
        {
            if (cryptogramSetting == null)
                cryptogramSetting = EditorTool.LoadSettingData<CryptogramSetting>();
            return cryptogramSetting;
        }
    }
}