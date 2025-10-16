using MyBox;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace OxGFrame.AssetLoader.Bundle
{
    [MovedFrom("CryptogramSetting")]
    [CreateAssetMenu(fileName = nameof(CryptogramSettings), menuName = "OxGFrame/Create Settings/Create Cryptogram Settings")]
    public class CryptogramSettings : ScriptableObject
    {
        [Separator("OFFSET")]
        public int dummySize = 1;

        [Separator("XOR")]
        public byte xorKey = 1;

        [Separator("HT2XOR")]
        public byte hXorKey = 1;
        public byte tXorKey = 1;
        public byte jXorKey = 1;

        [Separator("HT2XOR Plus")]
        public byte hXorPlusKey = 1;
        public byte tXorPlusKey = 1;
        public byte j1XorPlusKey = 1;
        public byte j2XorPlusKey = 1;

        [Separator("AES")]
        public string aesKey = "aes_key";
        public string aesIv = "aes_iv";

        [Separator("CHACHA20")]
        public string chacha20Key = "chacha20_key";
        public string chacha20Nonce = "chacha20_nonce";
        public uint chacha20Counter = 1;

        [Separator("XXTEA")]
        public string xxteaKey = "xxtea_key";

        [Separator("OFFSET XOR")]
        public byte offsetXorKey = 1;
        public int offsetXorDummySize = 1;
    }
}