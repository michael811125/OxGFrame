namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    public interface ISecuredString
    {
        public byte[] StringToBytes(string input);

        public string BytesToString();

        public byte[] GenerateSaltBytes(int length);
    }
}