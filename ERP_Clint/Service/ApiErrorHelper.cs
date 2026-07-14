using System.Text.Json;

namespace ERP_Clint.Service;

public static class ApiErrorHelper
{
    public static async Task<string> ReadMessageAsync(HttpResponseMessage response, string fallback)
    {
        var raw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(raw))
            return fallback;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("Message", out var msg))
                return msg.GetString() ?? fallback;
            if (doc.RootElement.TryGetProperty("message", out var msg2))
                return msg2.GetString() ?? fallback;
        }
        catch
        {
            // ignore
        }

        return raw.Length > 200 ? fallback : raw.Trim('"');
    }
}
