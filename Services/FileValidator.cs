using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;

namespace TravelPlannerApp.Services;

public static class FileValidator
{
    private static readonly Dictionary<string, byte[]> allowedSignatures = new()
    {
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D } }
    };

    public static bool IsValidFile(IBrowserFile file)
    {
        if (file == null || file.Size == 0) return false;

        var ext = Path.GetExtension(file.Name).ToLowerInvariant();

        if (!allowedSignatures.ContainsKey(ext)) return false;

        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        using var reader = new BinaryReader(stream);

        var expectedSignature = allowedSignatures[ext];
        var headerBytes = reader.ReadBytes(expectedSignature.Length);

        for (int i = 0; i < expectedSignature.Length; i++)
        {
            if (headerBytes[i] != expectedSignature[i])
            {
                return false;
            }
        }

        return true;
    }
}