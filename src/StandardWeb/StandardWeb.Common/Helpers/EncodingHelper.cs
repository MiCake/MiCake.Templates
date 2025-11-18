using System.IO.Compression;
using System.Text;

namespace StandardWeb.Common.Helpers;

public class EncodingHelper
{
    public static string DecodeGzip(Stream compressedData)
    {
        using var decompressionStream = new GZipStream(compressedData, CompressionMode.Decompress);
        using var reader = new StreamReader(decompressionStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 安全读取HTTP响应内容，处理编码问题
    /// </summary>
    public static async Task<string> ReadResponseSafelyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            // First try to read as string with default encoding
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseText;
        }
        catch (Exception)
        {
            // If that fails due to encoding issues, read as bytes and decode manually
            var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            // Try to detect encoding from Content-Type header
            var contentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty;
            var encoding = GetEncodingFromContentType(contentType);

            return encoding.GetString(responseBytes);
        }
    }

    /// <summary>
    /// 从Content-Type头中提取编码信息
    /// </summary>
    private static System.Text.Encoding GetEncodingFromContentType(string contentType)
    {
        try
        {
            // Look for charset parameter in Content-Type
            var charsetIndex = contentType.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);
            if (charsetIndex >= 0)
            {
                var charsetStart = charsetIndex + 8; // Length of "charset="
                var charsetEnd = contentType.IndexOf(';', charsetStart);
                if (charsetEnd == -1) charsetEnd = contentType.Length;

                var charset = contentType.Substring(charsetStart, charsetEnd - charsetStart).Trim();

                // Common charset mappings
                return charset.ToLowerInvariant() switch
                {
                    "utf-8" => Encoding.UTF8,
                    "gb2312" => Encoding.GetEncoding("GB2312"),
                    "gbk" => Encoding.GetEncoding("GBK"),
                    "big5" => Encoding.GetEncoding("Big5"),
                    "iso-8859-1" => Encoding.Latin1,
                    _ => Encoding.UTF8 // Default fallback
                };
            }
        }
        catch (Exception)
        {
            // If encoding detection fails, fall back to UTF-8
        }

        return Encoding.UTF8;
    }

    /// <summary>
    /// 检测字节数组的编码格式
    /// </summary>
    private static System.Text.Encoding DetectEncoding(byte[] bytes)
    {
        // Check for BOM
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8;

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode;

        // For Chinese content, try GB2312/GBK if UTF-8 detection fails
        try
        {
            var utf8String = Encoding.UTF8.GetString(bytes);
            // If UTF-8 decoding produces valid content, use it
            if (!utf8String.Contains('\uFFFD')) // No replacement characters
                return Encoding.UTF8;
        }
        catch
        {
            // UTF-8 failed, try GB2312
        }

        // Default to GB2312 for Chinese financial data
        try
        {
            return Encoding.GetEncoding("GB2312");
        }
        catch
        {
            return Encoding.UTF8; // Final fallback
        }
    }
}
