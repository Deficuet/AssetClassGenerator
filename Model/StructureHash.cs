using System;
using System.Security.Cryptography;
using System.Text;

namespace AssetClassGenerator.Model;

public static class StructureHash
{
    public static string Compute(string signature)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(signature));
        var builder = new StringBuilder(8);
        for (int i = 0; i < 4; ++i)
        {
            builder.Append(hash[i].ToString("x2"));
        }
        return builder.ToString();
    }
}
