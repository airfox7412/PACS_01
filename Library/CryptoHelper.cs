using System;
using System.Security.Cryptography;
using System.Text;

namespace Api.Library;

/// <summary>
/// 
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// 產生 Hash256 編碼
    /// </summary>
    /// <param name="source"></param>
    /// <param name="privateKey"></param>
    /// <returns></returns>
    public static string ToHash256(string source, string privateKey = "DataCom.123.qwe")
    {
        using var sha2 = SHA256.Create();
        var hash = sha2.ComputeHash(Encoding.UTF8.GetBytes(source));
        var sb = new StringBuilder(hash.Length * 2);

        foreach (byte b in hash)
        {
            // can be "x2" if you want lowercase
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string ToMd5(string source)
    {
        using var cryptoMd5 = MD5.Create();
        //將字串編碼成 UTF8 位元組陣列
        var bytes = Encoding.UTF8.GetBytes(source);

        //取得雜湊值位元組陣列
        var hash = cryptoMd5.ComputeHash(bytes);

        //取得 MD5
        return BitConverter.ToString(hash)
            .Replace("-", String.Empty)
            .ToUpper();
    }
}