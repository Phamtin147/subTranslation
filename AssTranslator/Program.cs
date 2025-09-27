using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    // CONFIG
    private const string Model = "gemini-2.0-flash";
    private const int BatchSize = 25; // Giảm batch size để tránh lỗi JSON
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromMinutes(2);

    // Default prompt — bạn có thể chỉnh bằng file custom_prompt.txt
    private const string DefaultPrompt = """
Bạn là dịch giả phụ đề chuyên nghiệp.
- Dịch EN -> VI, tự nhiên, ngắn gọn.
- Tên riêng/vật phẩm/địa danh: kèm Hán Việt trong ngoặc nếu phù hợp.
- Giữ nguyên [TAGx] và {\...} nguyên dạng.
- Thay \N bằng [BR] khi trả; chương trình sẽ đổi lại.
- Đầu vào: JSON mảng [{ "id": "...", "text": "..." }]
- Trả về: JSON mảng [{ "id": "...", "vi": "..." }]
CHỈ TRẢ JSON, KHÔNG GIẢI THÍCH.
""";
    //Bạn là dịch giả phụ đề phim. Nhiệm vụ:
    //- Dịch từ tiếng Anh sang TIẾNG VIỆT TỰ NHIÊN.
    //- Với tên nhân vật, địa danh, tổ chức, vật phẩm, vũ khí, bí danh... hãy thêm Hán Việt trong ngoặc: ví dụ "Ling Cage (Linh Quật)" nếu phù hợp. Giữ nhất quán xuyên suốt.
    //- Tuyệt đối giữ nguyên mọi PLACEHOLDER và TAG trong văn bản: ví dụ {\i1}, {\pos(320,240)} — phải giữ y nguyên.
    //- Thay \N bằng [BR] khi gửi và model phải trả về [BR] cho xuống dòng; chương trình sẽ chuyển lại thành \N.
    //- Không thêm dòng mới hay xóa dòng; chỉ trả mảng JSON.
    //- Đầu vào: JSON mảng các đối tượng: [{ "id": string, "text": string }]
    //- Đầu ra: JSON mảng các đối tượng: [{ "id": string, "vi": string }]
    //Chỉ trả đúng JSON, KHÔNG trả lời thêm.

    // Regex bắt Dialogue line (lấy phần text ở cuối)
    private static readonly Regex DialogueRe = new Regex(@"^(Dialogue:\s*\d+,\s*[^,]*,\s*[^,]*,\s*[^,]*,\s*[^,]*,\s*[^,]*,\s*[^,]*,\s*[^,]*,\s*[^,]*,)(.*)$", RegexOptions.Compiled);

    // Mask ASS tags like {\i1} {\b1} {\pos(..)} -> [TAG0]
    private static readonly Regex AssTagRe = new Regex(@"\{\\.*?\}", RegexOptions.Compiled);

    static async Task<int> Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet run -- <input.ass> [optional:custom_prompt.txt]");
            return 1;
        }

        string inputPath = args[0];
        string promptPath = args.Length >= 2 ? args[1] : null;

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input file not found: {inputPath}");
            return 2;
        }

        string apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.Error.WriteLine("ERROR: Please set environment variable GOOGLE_API_KEY with your Gemini API key.");
            return 3;
        }

        string prompt = DefaultPrompt;
        if (!string.IsNullOrEmpty(promptPath) && File.Exists(promptPath))
        {
            prompt = await File.ReadAllTextAsync(promptPath, Encoding.UTF8);
        }

        // Read file
        var allLines = await File.ReadAllLinesAsync(inputPath, Encoding.UTF8);

        // Collect dialogues
        var dialogues = new List<DialogueItem>();
        for (int i = 0; i < allLines.Length; i++)
        {
            var line = allLines[i];
            var m = DialogueRe.Match(line);
            if (!m.Success) continue;
            var head = m.Groups[1].Value; // everything up to the text field (including trailing comma)
            var text = m.Groups[2].Value;

            // Mask tags
            var (masked, map) = MaskTags(text);
            masked = masked.Replace("\\N", "[BR]").Replace("\\n", "[br]");

            dialogues.Add(new DialogueItem
            {
                LineIndex = i,
                Head = head,
                OriginalText = text,
                MaskedText = masked,
                TagMap = map
            });
        }

        Console.WriteLine($"Found {dialogues.Count} Dialogue lines.");

        // Prepare HttpClient
        using var http = new HttpClient { Timeout = HttpTimeout };

        var translatedMap = new Dictionary<int, string>(); // LineIndex -> full new Dialogue line

        // Process in batches of BatchSize
        for (int offset = 0; offset < dialogues.Count; offset += BatchSize)
        {
            var batch = dialogues.Skip(offset).Take(BatchSize).ToList();
            Console.WriteLine($"Processing batch {offset / BatchSize + 1}: {batch.Count} lines...");

            // Prepare input JSON array
            var inputs = batch.Select((d, idx) => new { id = $"L{offset + idx}", text = d.MaskedText }).ToArray();
            string inputsJson = JsonSerializer.Serialize(inputs, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            // Build user content combining prompt + INPUT
            var sb = new StringBuilder();
            sb.AppendLine(prompt);
            sb.AppendLine();
            sb.AppendLine("INPUT:");
            sb.AppendLine(inputsJson);

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] { new { text = sb.ToString() } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    topP = 0.9,
                    maxOutputTokens = 8192
                }
            };

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={apiKey}";
            var requestJson = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            // Retry mechanism for API calls
            int maxRetries = 10;
            int retryDelay = 5000; // 5 seconds
            HttpResponseMessage? resp = null;
            string? respBody = null;
            
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                    };

                    resp = await http.SendAsync(req);
                    respBody = await resp.Content.ReadAsStringAsync();

                    if (resp.IsSuccessStatusCode)
                    {
                        break; // Success, exit retry loop
                    }
                    
                    // Check if it's a retryable error
                    if (resp.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || 
                        resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                        resp.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        if (retry < maxRetries - 1)
                        {
                            Console.WriteLine($"API Error {resp.StatusCode}, retrying in {retryDelay/1000} seconds... (attempt {retry + 1}/{maxRetries})");
                            await Task.Delay(retryDelay);
                            retryDelay *= 2; // Exponential backoff
                            continue;
                        }
                    }
                    
                    // Non-retryable error or max retries reached
                    Console.Error.WriteLine($"API Error: {resp.StatusCode}\n{respBody}");
                    return 4;
                }
                catch (Exception ex)
                {
                    if (retry < maxRetries - 1)
                    {
                        Console.WriteLine($"Network error, retrying in {retryDelay/1000} seconds... (attempt {retry + 1}/{maxRetries})");
                        Console.WriteLine($"Error: {ex.Message}");
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        continue;
                    }
                    else
                    {
                        Console.Error.WriteLine($"Network error after {maxRetries} attempts: {ex.Message}");
                        return 4;
                    }
                }
            }

            if (resp == null || !resp.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Failed to get response after {maxRetries} attempts");
                return 4;
            }

            // Parse response text: find generated text in candidates[0].content.parts[0].text
            using var doc = JsonDocument.Parse(respBody);
            var root = doc.RootElement;
            string modelText = "";
            try
            {
                modelText = root.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Cannot parse model response JSON structure. Raw response:");
                Console.Error.WriteLine(respBody);
                Console.Error.WriteLine(ex);
                return 5;
            }

            if (string.IsNullOrWhiteSpace(modelText))
            {
                Console.Error.WriteLine("Empty response from model.");
                return 6;
            }

            // Expect modelText is a JSON array string. Try parse it.
            List<TranslatedItem>? translatedList = null;
            try
            {
                translatedList = JsonSerializer.Deserialize<List<TranslatedItem>>(modelText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception)
            {
                // If model text contains stray characters, try to find JSON substring
                var jsonStart = modelText.IndexOf('[');
                var jsonEnd = modelText.LastIndexOf(']');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var sub = modelText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    try
                    {
                        translatedList = JsonSerializer.Deserialize<List<TranslatedItem>>(sub, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch (Exception)
                    {
                        // Try to fix incomplete JSON by adding missing closing brackets
                        var fixedJson = sub;
                        var openBraces = fixedJson.Count(c => c == '{');
                        var closeBraces = fixedJson.Count(c => c == '}');
                        var openBrackets = fixedJson.Count(c => c == '[');
                        var closeBrackets = fixedJson.Count(c => c == ']');
                        
                        // Add missing closing braces and brackets
                        while (openBraces > closeBraces)
                        {
                            fixedJson += "}";
                            closeBraces++;
                        }
                        while (openBrackets > closeBrackets)
                        {
                            fixedJson += "]";
                            closeBrackets++;
                        }
                        
                        try
                        {
                            translatedList = JsonSerializer.Deserialize<List<TranslatedItem>>(fixedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine("Failed to parse JSON even after fixing. Raw output:");
                            Console.Error.WriteLine(modelText);
                            Console.Error.WriteLine("Fixed JSON:");
                            Console.Error.WriteLine(fixedJson);
                            Console.Error.WriteLine(e);
                            return 7;
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine("Model did not return JSON array. Raw output:");
                    Console.Error.WriteLine(modelText);
                    return 8;
                }
            }

            // Map back translations
            foreach (var t in translatedList!)
            {
                // id like L{globalIndex}
                if (!t.Id.StartsWith("L"))
                {
                    Console.Error.WriteLine($"Unexpected id format: {t.Id}");
                    continue;
                }
                if (!int.TryParse(t.Id.Substring(1), out int globalIndex))
                {
                    Console.Error.WriteLine($"Cannot parse id number: {t.Id}");
                    continue;
                }
                int batchIndex = globalIndex - offset;
                if (batchIndex < 0 || batchIndex >= batch.Count)
                {
                    // fallback: try find by matching text length — not ideal, skip
                    continue;
                }

                var dlg = batch[batchIndex];

                // Restore [BR] -> \N and unmask tags
                var vi = t.Vi.Replace("[BR]", "\\N").Replace("[br]", "\\n");
                vi = UnmaskTags(vi, dlg.TagMap);

                // Compose full Dialogue line
                var newLine = dlg.Head + vi;
                translatedMap[dlg.LineIndex] = newLine;
            }

            Console.WriteLine($"Batch {offset / BatchSize + 1} processed.");
        }

        // Write output file: copy original lines and replace dialogue lines
        var outLines = (string[])allLines.Clone();
        foreach (var kv in translatedMap)
        {
            outLines[kv.Key] = kv.Value;
        }

        // Output file name: <originalName>.vi.ass
        var outPath = Path.Combine(Path.GetDirectoryName(inputPath) ?? ".", Path.GetFileNameWithoutExtension(inputPath) + ".vi.ass");
        await File.WriteAllLinesAsync(outPath, outLines, new UTF8Encoding(false));
        Console.WriteLine($"Done. Output saved: {outPath}");
        return 0;
    }

    // Helper: mask tags -> returns masked text and map
    private static (string masked, List<string> map) MaskTags(string text)
    {
        var map = new List<string>();
        string masked = AssTagRe.Replace(text, m =>
        {
            var idx = map.Count;
            map.Add(m.Value);
            return $"[TAG{idx}]";
        });
        return (masked, map);
    }

    // Helper: unmask tags
    private static string UnmaskTags(string text, List<string> map)
    {
        for (int i = 0; i < map.Count; i++)
            text = text.Replace($"[TAG{i}]", map[i]);
        return text;
    }

    private class DialogueItem
    {
        public int LineIndex { get; set; }
        public string Head { get; set; } = "";
        public string OriginalText { get; set; } = "";
        public string MaskedText { get; set; } = "";
        public List<string> TagMap { get; set; } = new();
    }

    private record TranslatedItem(string Id, string Vi);
}
