using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AssTranslator
{
    public class AssTranslator
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _customPrompt;
        private readonly string _languageCode;
        private readonly HttpClient _httpClient;
        private const int BatchSize = 89;
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";
        private const string DefaultPrompt = @"
Bạn là dịch giả phụ đề chuyên nghiệp.
- Sub bên trong là từ bộ phim hoạt hình Trung Quốc tên là LING CAGE còn gọi là LING LONG
- Dịch EN -> {0}.
- Tên riêng/vật phẩm/địa danh: kèm Hán Việt trong ngoặc nếu phù hợp.
- Phải để ý thật nhưng cách xưng hô như i you để tránh đang thoại của người con trai lớn tuổi hơn nhưng là xưng mình là em
- Tuyệt đối giữ nguyên mọi PLACEHOLDER và TAG trong văn bản: ví dụ {\i1}, {\pos(320,240)} — phải giữ y nguyên.
- Giữ nguyên [TAGx] và
{\...}
nguyên dạng.
- Thay \N bằng[BR] khi trả; chương trình sẽ đổi lại.
- Không thêm dòng mới hay xóa dòng; chỉ trả mảng JSON.
";

        public event EventHandler<int> ProgressChanged;
        public event EventHandler<string> StatusChanged;

        public AssTranslator(string apiKey, string model, string customPrompt, string languageCode)
        {
            _apiKey = apiKey;
            _model = model;
            _customPrompt = customPrompt;
            _languageCode = languageCode;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
        }

        public async Task<bool> TranslateFile(string inputFilePath, string outputFilePath)
        {
            try
            {
                OnStatusChanged("Đang đọc file phụ đề...");
                var lines = await File.ReadAllLinesAsync(inputFilePath);
                var dialogueLines = new List<string>();
                var nonDialogueLines = new List<string>();
                var dialogueIndices = new List<int>();

                // Tách các dòng dialogue và non-dialogue
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.StartsWith("Dialogue:"))
                    {
                        dialogueLines.Add(line);
                        dialogueIndices.Add(i);
                    }
                    else
                    {
                        nonDialogueLines.Add(line);
                    }
                }

                OnStatusChanged($"Đã tìm thấy {dialogueLines.Count} dòng thoại cần dịch");

                // Chuẩn bị các batch để dịch
                var batches = new List<List<string>>();
                for (int i = 0; i < dialogueLines.Count; i += BatchSize)
                {
                    batches.Add(dialogueLines.Skip(i).Take(BatchSize).ToList());
                }

                OnStatusChanged($"Chia thành {batches.Count} batch để dịch");

                var translatedDialogues = new List<string>();
                int batchCount = 0;

                // Dịch từng batch
                foreach (var batch in batches)
                {
                    batchCount++;
                    OnStatusChanged($"Đang dịch batch {batchCount}/{batches.Count}...");
                    
                    var extractedTexts = batch.Select(line => ExtractDialogueText(line)).ToList();
                    var translatedTexts = await TranslateTexts(extractedTexts);
                    
                    if (translatedTexts.Count != batch.Count)
                    {
                        OnStatusChanged($"Lỗi: Số lượng dòng dịch không khớp với số lượng dòng gốc ({translatedTexts.Count} != {batch.Count})");
                        return false;
                    }

                    for (int i = 0; i < batch.Count; i++)
                    {
                        var originalLine = batch[i];
                        var translatedText = translatedTexts[i].Replace("[BR]", "\\N");
                        var translatedLine = ReplaceDialogueText(originalLine, translatedText);
                        translatedDialogues.Add(translatedLine);
                    }

                    // Cập nhật tiến độ
                    int progressPercentage = (int)((double)batchCount / batches.Count * 100);
                    OnProgressChanged(progressPercentage);
                }

                // Tạo file đầu ra
                var outputLines = new List<string>();
                int dialogueIndex = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (dialogueIndices.Contains(i))
                    {
                        outputLines.Add(translatedDialogues[dialogueIndex]);
                        dialogueIndex++;
                    }
                    else
                    {
                        outputLines.Add(lines[i]);
                    }
                }

                OnStatusChanged("Đang lưu file phụ đề đã dịch...");
                await File.WriteAllLinesAsync(outputFilePath, outputLines);
                OnStatusChanged("Hoàn thành dịch phụ đề!");
                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Lỗi: {ex.Message}");
                return false;
            }
        }

        private string ExtractDialogueText(string dialogueLine)
        {
            var parts = dialogueLine.Split(',', 10);
            if (parts.Length < 10)
            {
                return string.Empty;
            }
            return parts[9];
        }

        private string ReplaceDialogueText(string dialogueLine, string newText)
        {
            var parts = dialogueLine.Split(',', 10);
            if (parts.Length < 10)
            {
                return dialogueLine;
            }
            parts[9] = newText;
            return string.Join(',', parts);
        }

        private async Task<List<string>> TranslateTexts(List<string> texts)
        {
            try
            {
                string prompt = string.IsNullOrEmpty(_customPrompt) 
                    ? string.Format(DefaultPrompt, _languageCode) 
                    : File.ReadAllText(_customPrompt);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new
                                {
                                    text = $"{prompt}\n\n{JsonSerializer.Serialize(texts)}"
                                }
                            }
                        }
                    }
                };

                var requestJson = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var url = string.Format(ApiUrl, _model, _apiKey);

                var response = await _httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    OnStatusChanged($"API Error: {responseJson}");
                    throw new Exception($"API Error: {response.StatusCode}");
                }

                var jsonDocument = JsonDocument.Parse(responseJson);
                var textElement = jsonDocument.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text");

                var responseText = textElement.GetString();
                if (string.IsNullOrEmpty(responseText))
                {
                    throw new Exception("Empty response from API");
                }

                // Tìm mảng JSON trong phản hồi
                var match = Regex.Match(responseText, @"\[\s*"".*""\s*\]");
                if (!match.Success)
                {
                    throw new Exception("Could not find JSON array in response");
                }

                var jsonArray = match.Value;
                var translatedTexts = JsonSerializer.Deserialize<List<string>>(jsonArray);
                return translatedTexts;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Translation Error: {ex.Message}");
                throw;
            }
        }

        protected virtual void OnProgressChanged(int progressPercentage)
        {
            ProgressChanged?.Invoke(this, progressPercentage);
        }

        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }
    }
}