global using System.Text.Json;
global using System.Text.Json.Serialization;

namespace Perigon.AspNetCore.Utils;

/// <summary>
/// 提供常用加解密方法
/// </summary>
public class HashCrypto
{
    public const int DefaultPbkdf2Iterations = 210_000;
    private const int Pbkdf2OutputLength = 32;
    private const int AesIvSize = 16;
    private const string AesPayloadPrefix = "v2:";
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    private static JsonSerializerOptions JsonSerializerOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

    /// <summary>
    /// SHA512 encrypt
    /// </summary>
    /// <param name="value"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public static string GeneratePwd(
        string value,
        string salt,
        int iterations = DefaultPbkdf2Iterations
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        var valueBytes = Rfc2898DeriveBytes.Pbkdf2(
            password: value,
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: Pbkdf2OutputLength
        );
        return Convert.ToBase64String(valueBytes);
    }

    /// <summary>
    /// 生成PAT
    /// </summary>
    /// <param name="value"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public static string GeneratePAT(
        string value,
        int iterations = DefaultPbkdf2Iterations
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        var salt = BuildSalt();
        var valueBytes = Rfc2898DeriveBytes.Pbkdf2(
            password: value,
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: Pbkdf2OutputLength
        );
        return Convert.ToBase64String(valueBytes);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="value"></param>
    /// <param name="salt"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static bool Validate(
        string value,
        string salt,
        string hash,
        int iterations = DefaultPbkdf2Iterations
    )
    {
        try
        {
            var expected = Convert.FromBase64String(hash);
            var actual = Convert.FromBase64String(GeneratePwd(value, salt, iterations));
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// 生成盐
    /// </summary>
    /// <returns></returns>
    public static string BuildSalt()
    {
        var randomBytes = new byte[128 / 8];
        Rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// HMACSHA256 encrypt
    /// </summary>
    /// <param name="key"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string HMACSHA256(string key, string content)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var valueBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(valueBytes);
    }

    /// <summary>
    /// 字符串md5值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Md5Hash(string str)
    {
        return HashString(str);
    }

    /// <summary>
    /// 字符串hash值
    /// </summary>
    /// <param name="str"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string HashString(string str, HashType type = HashType.MD5)
    {
        var bytes = HashData(str, type);

        return Convert.ToHexStringLower(bytes);
    }

    private static byte[] HashData(string str, HashType type = HashType.MD5)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        return type switch
        {
            HashType.MD5 => MD5.HashData(bytes),
            HashType.SHA256 => SHA256.HashData(bytes),
            HashType.SHA512 => SHA512.HashData(bytes),
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// 某文件的md5值
    /// </summary>
    /// <param name="stream">file stream</param>
    /// <returns></returns>
    public static string Md5FileHash(Stream stream)
    {
        using var md5 = MD5.Create();
        var data = md5.ComputeHash(stream);
        StringBuilder sBuilder = new();

        foreach (var b in data)
        {
            _ = sBuilder.Append(b.ToString("x2"));
        }
        return sBuilder.ToString();
    }

    /// <summary>
    /// 生成随机数
    /// </summary>
    /// <param name="length"></param>
    /// <param name="useNum"></param>
    /// <param name="useLow"></param>
    /// <param name="useUpp"></param>
    /// <param name="useSpe"></param>
    /// <param name="custom"></param>
    /// <returns></returns>
    public static string GetRandom(
        int length = 4,
        bool useNum = true,
        bool useLow = false,
        bool useUpp = true,
        bool useSpe = false,
        string custom = ""
    )
    {

        var sb = new StringBuilder(custom);
        if (useNum)
        {
            sb.Append("0123456789");
        }
        if (useLow)
        {
            sb.Append("abcdefghijklmnopqrstuvwxyz");
        }
        if (useUpp)
        {
            sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }
        if (useSpe)
        {
            sb.Append("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~");
        }

        ReadOnlySpan<char> strSpan = sb.ToString().AsSpan();
        var resultBuilder = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            var position = RandomNumberGenerator.GetInt32(strSpan.Length);
            resultBuilder.Append(strSpan[position]);
        }
        return resultBuilder.ToString();
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="text">源文</param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string AesEncrypt(string text, string key, byte[]? iv = null)
    {
        var actualIv = iv ?? RandomNumberGenerator.GetBytes(AesIvSize);
        ValidateAesIv(actualIv);

        var bytes = Encoding.UTF8.GetBytes(text);
        var encrypted = EncryptAes(bytes, GetAesKey(key), actualIv);
        var payload = new byte[AesIvSize + encrypted.Length];
        Buffer.BlockCopy(actualIv, 0, payload, 0, AesIvSize);
        Buffer.BlockCopy(encrypted, 0, payload, AesIvSize, encrypted.Length);
        return AesPayloadPrefix + Convert.ToBase64String(payload);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string AesDecrypt(string cipherText, string key, byte[]? iv = null)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return string.Empty;
        }

        if (cipherText.StartsWith(AesPayloadPrefix, StringComparison.Ordinal))
        {
            var payload = Convert.FromBase64String(cipherText[AesPayloadPrefix.Length..]);
            if (payload.Length <= AesIvSize)
            {
                throw new CryptographicException("Invalid AES payload.");
            }

            var actualIv = payload[..AesIvSize];
            var encrypted = payload[AesIvSize..];
            return DecryptAes(encrypted, GetAesKey(key), actualIv);
        }

        var legacyCipher = Convert.FromBase64String(cipherText);
        if (iv is not null)
        {
            ValidateAesIv(iv);
            return DecryptAes(legacyCipher, GetAesKey(key), iv);
        }

        return DecryptAes(legacyCipher, GetLegacyAesKey(key), GetLegacyAesIv(key));
    }

    private static byte[] EncryptAes(byte[] bytes, byte[] key, byte[] iv)
    {
        using var aesAlg = CreateAes(key, iv);
        using ICryptoTransform encryptor = aesAlg.CreateEncryptor();
        using MemoryStream memoryStream = new();
        using var csEncrypt = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        csEncrypt.Write(bytes, 0, bytes.Length);
        csEncrypt.FlushFinalBlock();
        return memoryStream.ToArray();
    }

    private static string DecryptAes(byte[] encrypted, byte[] key, byte[] iv)
    {
        using var aesAlg = CreateAes(key, iv);
        using ICryptoTransform decryptor = aesAlg.CreateDecryptor();
        using MemoryStream msDecrypt = new(encrypted);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt, Encoding.UTF8);
        return srDecrypt.ReadToEnd();
    }

    private static Aes CreateAes(byte[] key, byte[] iv)
    {
        var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private static byte[] GetAesKey(string key)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }

    private static byte[] GetLegacyAesKey(string key)
    {
        return Encoding.UTF8.GetBytes(Md5Hash(key));
    }

    private static byte[] GetLegacyAesIv(string key)
    {
        return GetLegacyAesKey(key)[..AesIvSize];
    }

    private static void ValidateAesIv(byte[] iv)
    {
        if (iv.Length != AesIvSize)
        {
            throw new ArgumentException($"AES IV must be {AesIvSize} bytes.", nameof(iv));
        }
    }

    /// <summary>
    /// json对象加密
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string JsonEncrypt(object data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonSerializerOptions);

        if (bytes != null)
        {
            bytes = bytes.Select(b => b == byte.MaxValue ? byte.MinValue : (byte)(b + 1)).ToArray();
            Array.Reverse(bytes);
            return Convert.ToBase64String(bytes);
        }
        return string.Empty;
    }

    /// <summary>
    /// json对象解密
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T? JsonDecrypt<T>(string value)
        where T : class
    {
        var bytes = Convert.FromBase64String(value);
        if (bytes != null)
        {
            Array.Reverse(bytes);
            bytes = bytes.Select(b => b == byte.MinValue ? byte.MaxValue : (byte)(b - 1)).ToArray();
            var jsonString = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
        }
        return null;
    }
}

/// <summary>
/// hash type
/// </summary>
public enum HashType
{
    MD5,
    SHA256,
    SHA512,
}
