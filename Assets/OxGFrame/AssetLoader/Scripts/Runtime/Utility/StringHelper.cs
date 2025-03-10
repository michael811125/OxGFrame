using System.Text;

namespace OxGFrame.AssetLoader.Utility
{
    public static class StringHelper
    {
        public static byte[] StringToBytes(string inputString)
        {
            return Encoding.UTF8.GetBytes(inputString);
        }

        public static string BytesToString(byte[] stringBytes)
        {
            return Encoding.UTF8.GetString(stringBytes);
        }
    }
}