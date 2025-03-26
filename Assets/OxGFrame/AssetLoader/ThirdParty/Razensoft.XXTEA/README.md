Razensoft.XXTEA
======================================================

A simple and fast zero-dependency cryptography cypher for Unity

## ATTENTION

This cypher is NOT a drop-in replacement for more secure cyphers, like AES. The attack on XXTEA is possible, but it is very expensive, on the verge of the feasible and probably not applicable to most "commercial" situations. Please get familiar with [the specifics](https://en.wikipedia.org/wiki/XXTEA) before using it in a more security-sensitive scenarios.

## DISCLAIMER

The code in this library is based on the code from [xxtea-dotnet](https://github.com/xxtea/xxtea-dotnet) repository.

Differences between **Razensoft.XXTEA** and **xxtea-dotnet**:

- Renamed root namespace from `Xxtea` to `Razensoft`
- Removed Base64 methods
- Made constructor public for cypher key caching

## Installation

There are several ways to install this library into our project:

- **Plain install**: Clone or [download](https://github.com/Razenpok/Razensoft.XXTEA/archive/master.zip) this repository and put it somewhere in your Unity project
- **Unity Package Manager (UPM)**: Add the following line to *Packages/manifest.json*:
  - `"com.razensoft.xxtea": "https://github.com/Razenpok/Razensoft.XXTEA.git?path=src/Razensoft.XXTEA#1.0.0",`
- **[OpenUPM](https://openupm.com)**: After installing [openupm-cli](https://github.com/openupm/openupm-cli), run the following command:
  - `openupm add com.razensoft.xxtea`

## Usage

```c#
using Razensoft;

public static void Main()
{
    var stringKey = "3GU45RUJR58xHub9";
    var stringData = "Lorem ipsum dolor sit amet";

    // You can use static methods.
    var encryptedString = XXTEA.Encrypt(stringData, stringKey);
    var decryptedString = XXTEA.DecryptString(encryptedString, stringKey);

    // Or create an instance to have the same key for all operations.
    var xxtea = new XXTEA(stringKey);
    encryptedString = xxtea.Encrypt(stringData);
    decryptedString = xxtea.DecryptString(encryptedString);

    // Key and data can be a string or a byte array.
    var byteKey = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
    encryptedString = XXTEA.Encrypt(stringData, byteKey);
    decryptedString = XXTEA.DecryptString(encryptedString, byteKey);

    var byteData = new byte[] { 11, 22, 33, 44, 55, 66, 77, 88, 99, 100, 110, 120, 130, 140, 150, 160};
    var encryptedBytes = XXTEA.Encrypt(byteData, byteKey);
    var decryptedBytes = XXTEA.Decrypt(encryptedBytes, byteKey);
}
```

## Contributors
A big thanks to the original author of [xxtea-dotnet](https://github.com/xxtea/xxtea-dotnet), [Ma Bingyao](https://github.com/andot). Don't forget to check it out!
