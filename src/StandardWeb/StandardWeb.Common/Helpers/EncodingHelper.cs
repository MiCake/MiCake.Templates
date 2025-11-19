using System.IO.Compression;
using System.Text;

namespace StandardWeb.Common.Helpers;

/// <summary>
/// Provides utilities for handling different text encodings and compression.
/// Particularly useful for HTTP responses with various character sets.
/// </summary>
public class EncodingHelper
{
    /// <summary>
    /// Decompresses GZIP-compressed stream to string.
    /// </summary>
    /// <param name="compressedData">GZIP-compressed data stream</param>
    /// <returns>Decompressed string content (UTF-8)</returns>
    public static string DecodeGzip(Stream compressedData)
    {
        using var decompressionStream = new GZipStream(compressedData, CompressionMode.Decompress);
        using var reader = new StreamReader(decompressionStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Safely reads HTTP response content with automatic encoding detection.
    /// Falls back to manual byte-level decoding if standard reading fails.
    /// </summary>
    /// <param name="response">HTTP response message to read</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response content as string</returns>
    public static async Task<string> ReadResponseSafelyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            // Attempt standard string reading with default encoding
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseText;
        }
        catch (Exception)
        {
            // Handle encoding issues by reading raw bytes and decoding manually
            var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            // Extract encoding from Content-Type header
            var contentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty;
            var encoding = GetEncodingFromContentType(contentType);

            return encoding.GetString(responseBytes);
        }
    }

    /// <summary>
    /// Extracts character encoding from Content-Type header.
    /// Supports common encodings: UTF-8, GB2312, GBK, Big5, ISO-8859-1.
    /// </summary>
    /// <param name="contentType">Content-Type header value</param>
    /// <returns>Detected encoding or UTF-8 as fallback</returns>
    private static System.Text.Encoding GetEncodingFromContentType(string contentType)
    {
        try
        {
            // Look for charset parameter in Content-Type header
            var charsetIndex = contentType.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);
            if (charsetIndex >= 0)
            {
                var charsetStart = charsetIndex + 8; // Length of "charset="
                var charsetEnd = contentType.IndexOf(';', charsetStart);
                if (charsetEnd == -1) charsetEnd = contentType.Length;

                var charset = contentType.Substring(charsetStart, charsetEnd - charsetStart).Trim();

                // Map common charset names to .NET Encoding objects
                return charset.ToLowerInvariant() switch
                {
                    "utf-8" => Encoding.UTF8,
                    "gb2312" => Encoding.GetEncoding("GB2312"),
                    "gbk" => Encoding.GetEncoding("GBK"),
                    "big5" => Encoding.GetEncoding("Big5"),
                    "iso-8859-1" => Encoding.Latin1,
                    _ => Encoding.UTF8 // Default fallback for unknown charsets
                };
            }
        }
        catch (Exception)
        {
            // Silently fall back to UTF-8 if encoding detection fails
        }

        return Encoding.UTF8;
    }

    /// <summary>
    /// Detects encoding from byte order mark (BOM) or content analysis.
    /// Useful for files without explicit encoding specification.
    /// </summary>
    /// <param name="bytes">Byte array to analyze</param>
    /// <returns>Detected encoding or UTF-8 as fallback</returns>
    private static System.Text.Encoding DetectEncoding(byte[] bytes)
    {
        // Check for UTF-8 BOM (EF BB BF)
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8;

        // Check for UTF-16 LE BOM (FF FE)
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;

        // Check for UTF-16 BE BOM (FE FF)
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode;

        // For Chinese content, try GB2312/GBK if UTF-8 detection fails
        try
        {
            var utf8String = Encoding.UTF8.GetString(bytes);
            // Check if UTF-8 decoding produced valid content (no replacement characters)
            if (!utf8String.Contains('\uFFFD'))
                return Encoding.UTF8;
        }
        catch
        {
            // UTF-8 decoding failed, will try GB2312
        }

        // Default to GB2312 for Chinese content
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
