using MyBox;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    [CreateAssetMenu(fileName = nameof(CryptogramSetting), menuName = "OxGFrame/Create Settings/Create Cryptogram Setting")]
    public class CryptogramSetting : ScriptableObject
    {
        [Separator("OFFSET")]
        public int randomSeed = 1;
        public int dummySize = 1;

        [Separator("XOR")]
        public byte xorKey = 1;

        [Separator("HT2XOR")]
        public byte hXorKey = 1;
        public byte tXorKey = 1;
        public byte jXorKey = 1;

        [Separator("AES")]
        public string aesKey = "aes_key";
        public string aesIv = "aes_iv";
    }
}