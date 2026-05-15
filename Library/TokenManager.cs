using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Api.Repositories;

namespace Api.Library;

/// <summary>
/// JWT Token管理
/// </summary>
public class TokenManager
{
    /// <summary>
    /// the token's issuer.
    /// </summary>
    private static readonly string Issuer;
    private static readonly AsyncLocal<string> TokenGroupsId = new();

    /// <summary>
    /// 登入帳號使用中機構
    /// </summary>
    public static string UsingInstitutionNo
    {
        get => TokenGroupsId.Value;
        set => TokenGroupsId.Value = value;
    }

    private static readonly AsyncLocal<string> TokenUserDate = new();

    /// <summary>
    /// 登入帳號使用中機構
    /// </summary>
    public static string UserDate
    {
        get => TokenUserDate.Value;
        set => TokenUserDate.Value = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly string Secret =
        "Database/0c4PnLf44gm9yC4QejkZvqZyzinPPSg/02kRxkmnxOyCqL38uUI7uV2l7r/xzI9xwsigSwkbnn+nmxA==";

    static TokenManager()
    {
        Issuer = typeof(Program).Assembly.FullName?.Split(',')[0];
        Secret = $"{Issuer}@{Secret}";
    }
    
    /// <summary>
    /// CreateToken
    /// </summary>
    /// <param name="accountNo"></param>
    /// <param name="orgCode"></param>
    /// <returns></returns>
    public static string CreateToken(string accountNo, string orgCode)
    {
        var result = GenerateToken(accountNo, orgCode);
        return result;
    }

    /// <summary>
    /// GenerateToken
    /// </summary>
    /// <param name="user"></param>
    /// <param name="org"></param>
    /// <returns></returns>
    private static string GenerateToken(string user, string org)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var descriptor = new SecurityTokenDescriptor
        {
            //Issuer = Issuer,
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user)
            }),
            Expires = DateTime.UtcNow.AddMinutes(1440),
            SigningCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256Signature)
        };
        if (org != null)
            descriptor.Subject.AddClaim(new Claim(ClaimTypes.GroupSid, org));
        
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateJwtSecurityToken(descriptor));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ticket"></param>
    /// <returns></returns>
    private static ClaimsPrincipal GetPrincipal(string ticket)
    {
        try
        {
            ticket = ticket?.Replace("Bearer ", "");
            JwtSecurityTokenHandler tokenHandler = new();
            JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(ticket);
            if (jwtToken == null)
                return null;

            TokenValidationParameters parameters = new TokenValidationParameters
            {
                // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
                NameClaimType = ClaimTypes.NameIdentifier,
                // 一般我們都會驗證 Issuer
                ValidateIssuer = false,
                //ValidIssuer = TokenManager.Issuer,

                // 通常不太需要驗證 Audience
                ValidateAudience = false,
                //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

                // 一般我們都會驗證 Token 的有效期間
                ValidateLifetime = true,

                // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                ValidateIssuerSigningKey = false,

                RequireExpirationTime = true,
                ValidIssuer = Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret))
            };
            return tokenHandler.ValidateToken(ticket, parameters, out _);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ticket"></param>
    /// <returns></returns>
    protected string ValidateToken(string ticket)
    {
        var principal = GetPrincipal(ticket);
        if (principal == null)
            return null;

        ClaimsIdentity identity;
        try
        {
            identity = (ClaimsIdentity)principal.Identity;
        }
        catch (NullReferenceException)
        {
            return null;
        }

        if (identity != null)
        {
            Claim usernameClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            if (usernameClaim != null)
            {
                var username = usernameClaim.Value;
                return username;
            }
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    protected string RandomPermit(int length)
    {
        string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!$_-";
        char[] chars = new char[length];
        Random rd = new Random();

        for (int i = 0; i < length; i++)
        {
            //allowedChars -> 這個String ，隨機取得一個字，丟給chars[i]
            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
        }

        return new string(chars);
    }
    /// <summary>
    /// 產生 Hash256 編碼
    /// </summary>
    /// <param name="source"></param>
    /// <param name="privateKey"></param>
    /// <returns></returns>
    public static string ToHash256(string source, string privateKey = "1234.qwer")
    {
        var keyByte = Encoding.Default.GetBytes(privateKey);

        var hash256 = new HMACSHA256(keyByte);

        var b = Encoding.Default.GetBytes(source);

        var h = hash256.ComputeHash(b);
        var e = string.Empty;

        foreach (var r in h)
        {
            e += r.ToString("x2");
        }
        return e;
    }

    /// <summary>
    /// hash password with sha2
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hash256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb256 = new StringBuilder(hash256.Length * 2);
        foreach (var b in hash256)
            sb256.Append(b.ToString("X2")); // can be "x2" if you want lowercase
        return sb256.ToString();
    }

    /// <summary>
    /// 檢查密碼複雜度要求(12碼以上，大小寫英文、數字、符號)
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static bool IsPasswordComplex(string password)
    {
        // 要求密碼至少 12 個字符，包含大小寫字母、數字和符號
        const string pattern = @"^(?=.*\d)(?=.*[a-zA-Z])(?=.*\W).{12,}$";
        return Regex.IsMatch(password, pattern);
    }

    /// <summary>
    /// 取得PracId
    /// </summary>
    /// <param name="AccountNo"></param>
    /// <returns></returns>
    public static string Operationer(string AccountNo)
    {
        return $"PRAC.{UsingInstitutionNo}.{AccountNo}";
    }
}