using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace AssTranslator
{
    public partial class MainForm : Form
    {
        private string _apiKey = "";
        private string _selectedModel = "gemini-2.5-flash";
        private string _inputFilePath = "";
        private string _outputFilePath = "";
        private string _customPromptPath = "";
        private bool _isTranslating = false;
        private bool _shouldStop = false;
        private string _selectedLanguage = "Vietnamese";
        private bool _isDarkMode = false;
        private readonly Color _darkPrimary = Color.FromArgb(118, 109, 109);
        private readonly Color _darkSecondary = Color.FromArgb(31, 29, 29);
        private readonly Dictionary<string, string> _languageCodes = new Dictionary<string, string>
        {
            { "Vietnamese", "VI" },
            { "English", "EN" },
            { "Chinese", "ZH" },
            { "Japanese", "JP" },
            { "Korean", "KR" },
            { "French", "FR" },
            { "German", "DE" },
            { "Spanish", "ES" },
            { "Russian", "RU" }
        };
        private readonly List<string> _availableModels = new List<string>
        {
            "gemini-1.5-flash",
            "gemini-1.5-pro",
            "gemini-2.0-flash",
            "gemini-2.0-pro",
            "gemini-2.5-flash",
            "gemini-2.5-pro"
        };

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            InitializeModelComboBox();
            InitializeLanguageComboBox();
        }

        private void InitializeModelComboBox()
        {
            cboModel.Items.Clear();
            foreach (var model in _availableModels)
            {
                cboModel.Items.Add(model);
            }
            
            // Select the highest model by default
            cboModel.SelectedItem = _selectedModel;
        }

        private void InitializeLanguageComboBox()
        {
            cboLanguage.Items.Clear();
            foreach (var language in _languageCodes.Keys)
            {
                cboLanguage.Items.Add(language);
            }
            
            // Load saved language or select Vietnamese by default
            string savedLanguage = Properties.Settings.Default.Language;
            if (!string.IsNullOrEmpty(savedLanguage) && _languageCodes.ContainsKey(savedLanguage))
            {
                _selectedLanguage = savedLanguage;
            }
            
            cboLanguage.SelectedItem = _selectedLanguage;
        }

        private void LoadSettings()
        {
            // Load API key from environment variable if exists
            _apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "";
            txtApiKey.Text = _apiKey;

            // Load custom prompt path if exists
            _customPromptPath = Path.Combine(Application.StartupPath, "custom_prompt.txt");
            if (File.Exists(_customPromptPath))
            {
                txtCustomPrompt.Text = _customPromptPath;
            }
            
            // Batch size is now fixed to 50
            
            // Set default output folder
            txtOutputFile.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }


        private void AddLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddLog(message)));
                return;
            }

            lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            lstLog.SelectedIndex = lstLog.Items.Count - 1;
            lstLog.SelectedIndex = -1;
        }

        private void OnProgressChanged(object sender, int progressPercentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnProgressChanged(sender, progressPercentage)));
                return;
            }

            progressBar.Value = progressPercentage;
        }

        private void OnStatusChanged(object sender, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnStatusChanged(sender, status)));
                return;
            }

            lblStatus.Text = status;
        }

        private void SaveSettings()
        {
            // Save API key to environment variable
            Environment.SetEnvironmentVariable("GOOGLE_API_KEY", _apiKey, EnvironmentVariableTarget.User);
            
            // Save selected language
            Properties.Settings.Default.Language = _selectedLanguage;
            Properties.Settings.Default.Save();
        }

        private void ToggleDarkMode()
        {
            _isDarkMode = !_isDarkMode;
            
            if (_isDarkMode)
            {
                // Apply dark theme
                this.BackColor = _darkSecondary;
                this.ForeColor = Color.White;
                
                // Apply dark theme to all controls
                ApplyDarkThemeToControls(this.Controls);
            }
            else
            {
                // Apply light theme (default)
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                
                // Apply light theme to all controls
                ApplyLightThemeToControls(this.Controls);
            }
        }

        private void ApplyDarkThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.BackColor = _darkPrimary;
                    textBox.ForeColor = Color.White;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.BackColor = _darkPrimary;
                    comboBox.ForeColor = Color.White;
                }
                else if (control is ListBox listBox)
                {
                    listBox.BackColor = _darkPrimary;
                    listBox.ForeColor = Color.White;
                }
                else if (control is Button button)
                {
                    button.BackColor = _darkPrimary;
                    button.ForeColor = Color.White;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = _darkPrimary;
                }
                else if (control is Label label)
                {
                    label.ForeColor = Color.White;
                }
                else if (control is ProgressBar progressBar)
                {
                    progressBar.BackColor = _darkPrimary;
                    progressBar.ForeColor = Color.White;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = _darkPrimary;
                    numericUpDown.ForeColor = Color.White;
                }
                
                // Recursively apply to child controls
                if (control.HasChildren)
                {
                    ApplyDarkThemeToControls(control.Controls);
                }
            }
        }

        private void ApplyLightThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.BackColor = SystemColors.Window;
                    textBox.ForeColor = SystemColors.WindowText;
                    textBox.BorderStyle = BorderStyle.Fixed3D;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.BackColor = SystemColors.Window;
                    comboBox.ForeColor = SystemColors.WindowText;
                }
                else if (control is ListBox listBox)
                {
                    listBox.BackColor = SystemColors.Window;
                    listBox.ForeColor = SystemColors.WindowText;
                }
                else if (control is Button button)
                {
                    button.BackColor = SystemColors.Control;
                    button.ForeColor = SystemColors.ControlText;
                    button.FlatStyle = FlatStyle.Standard;
                }
                else if (control is Label label)
                {
                    label.ForeColor = SystemColors.ControlText;
                }
                else if (control is ProgressBar progressBar)
                {
                    progressBar.BackColor = SystemColors.Control;
                    progressBar.ForeColor = SystemColors.Highlight;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = SystemColors.Window;
                    numericUpDown.ForeColor = SystemColors.WindowText;
                }
                
                // Recursively apply to child controls
                if (control.HasChildren)
                {
                    ApplyLightThemeToControls(control.Controls);
                }
            }
        }

        private async void btnTranslate_Click(object sender, EventArgs e)
        {
            if (_isTranslating)
            {
                MessageBox.Show("Đang trong quá trình dịch, vui lòng đợi hoàn thành.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                MessageBox.Show("Vui lòng nhập API Key trước khi dịch.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (lstInputFiles.Items.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một file phụ đề cần dịch.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtOutputFile.Text))
            {
                MessageBox.Show("Vui lòng chọn nơi lưu file phụ đề đã dịch.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _isTranslating = true;
                _shouldStop = false;
                btnTranslate.Enabled = false;
                btnStop.Visible = true;
                progressBar.Visible = true;
                lblStatus.Visible = true;

                var apiKey = txtApiKey.Text;
                var model = cboModel.SelectedItem?.ToString() ?? "";
                var language = cboLanguage.SelectedItem?.ToString() ?? "";
                var languageCode = _languageCodes.ContainsKey(language) ? _languageCodes[language] : "VI";
                var customPrompt = string.IsNullOrWhiteSpace(txtCustomPrompt.Text) ? null : txtCustomPrompt.Text;
                var outputFolder = txtOutputFile.Text;

                var apiProvider = "Gemini"; // Default to Gemini for now
                var retryCount = (int)numRetryCount.Value;
                var retryDelay = (int)numRetryDelay.Value;
                // Fixed batch size to 50
                var batchSize = 50;
                AddLog($"Sử dụng batch size cố định: {batchSize}");

                var translator = new AssTranslator(apiKey, model, customPrompt ?? "", languageCode, apiProvider, retryCount, retryDelay, batchSize);

                translator.ProgressChanged += OnProgressChanged;
                translator.StatusChanged += OnStatusChanged;

                var inputFiles = new List<string>();
                foreach (var item in lstInputFiles.Items)
                {
                    if (item != null)
                    {
                        var itemString = item.ToString();
                        if (!string.IsNullOrEmpty(itemString))
                        {
                            inputFiles.Add(itemString);
                        }
                    }
                }

                int totalFiles = inputFiles.Count;
                int completedFiles = 0;

                foreach (var inputFile in inputFiles)
                {
                    // Kiểm tra nếu user muốn dừng
                    if (_shouldStop)
                    {
                        AddLog("Đã dừng quá trình dịch theo yêu cầu người dùng");
                        break;
                    }

                    var fileName = Path.GetFileNameWithoutExtension(inputFile);
                    var outputFile = Path.Combine(outputFolder, $"{fileName}.{languageCode}.ass");

                    AddLog($"Đang dịch: {Path.GetFileName(inputFile)}");

                    try
                    {
                        var success = await translator.TranslateFile(inputFile, outputFile);

                        if (success)
                        {
                            completedFiles++;
                            AddLog($"✓ Hoàn thành: {Path.GetFileName(inputFile)}");
                        }
                        else
                        {
                            AddLog($"✗ Lỗi khi dịch: {Path.GetFileName(inputFile)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog($"✗ Lỗi khi dịch {Path.GetFileName(inputFile)}: {ex.Message}");
                    }

                    int overallProgress = (int)((double)completedFiles / totalFiles * 100);
                    progressBar.Value = overallProgress;
                    lblStatus.Text = $"Tiến độ: {completedFiles}/{totalFiles} files ({overallProgress}%)";
                }

                if (completedFiles == totalFiles)
                {
                    MessageBox.Show($"Dịch thành công {completedFiles}/{totalFiles} files!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Hoàn thành {completedFiles}/{totalFiles} files. Có {totalFiles - completedFiles} file bị lỗi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isTranslating = false;
                btnTranslate.Enabled = true;
                btnStop.Visible = false;
            }
        }

        private void Translator_ProgressChanged(object sender, int progressPercentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Translator_ProgressChanged(sender, progressPercentage)));
                return;
            }

            progressBar.Value = progressPercentage;
        }

        private void Translator_StatusChanged(object sender, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Translator_StatusChanged(sender, status)));
                return;
            }

            lblStatus.Text = status;
        }

        private void txtApiKey_TextChanged(object sender, EventArgs e)
        {
            _apiKey = txtApiKey.Text;
        }

        private void cboModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedModel = cboModel.SelectedItem.ToString();
        }
        
        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboLanguage.SelectedItem != null)
            {
                _selectedLanguage = cboLanguage.SelectedItem.ToString();
                UpdateOutputFilePath();
                Properties.Settings.Default.Language = _selectedLanguage;
                Properties.Settings.Default.Save();
            }
        }

        // New methods for multiple file handling
        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "ASS Files (*.ass)|*.ass|All Files (*.*)|*.*";
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Chọn file phụ đề cần dịch";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        if (!lstInputFiles.Items.Contains(fileName))
                        {
                            lstInputFiles.Items.Add(fileName);
                        }
                    }
                    AddLog($"Đã thêm {openFileDialog.FileNames.Length} file(s)");
                }
            }
        }

        private void btnRemoveFile_Click(object sender, EventArgs e)
        {
            if (lstInputFiles.SelectedItems.Count > 0)
            {
                var selectedItems = new List<object>();
                foreach (var item in lstInputFiles.SelectedItems)
                {
                    selectedItems.Add(item);
                }

                foreach (var item in selectedItems)
                {
                    lstInputFiles.Items.Remove(item);
                }
                AddLog($"Đã xóa {selectedItems.Count} file(s)");
            }
        }

        private void btnClearFiles_Click(object sender, EventArgs e)
        {
            int count = lstInputFiles.Items.Count;
            lstInputFiles.Items.Clear();
            AddLog($"Đã xóa tất cả {count} file(s)");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "ASS Files (*.ass)|*.ass|All Files (*.*)|*.*";
                openFileDialog.Title = "Chọn file phụ đề cần dịch";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _inputFilePath = openFileDialog.FileName;
                    // txtInputFile.Text = _inputFilePath; // Removed - using lstInputFiles now
                    UpdateOutputFilePath();
                }
            }
        }

        private void UpdateOutputFilePath()
        {
            if (!string.IsNullOrEmpty(_inputFilePath))
            {
                string languageCode = _languageCodes[_selectedLanguage].ToLower();
                string directory = Path.GetDirectoryName(_inputFilePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_inputFilePath);
                string extension = Path.GetExtension(_inputFilePath);
                
                _outputFilePath = Path.Combine(directory, $"{fileNameWithoutExt}.{languageCode}{extension}");
                txtOutputFile.Text = _outputFilePath;
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Chọn thư mục để lưu file phụ đề đã dịch";
                folderDialog.ShowNewFolderButton = true;
                
                if (!string.IsNullOrEmpty(txtOutputFile.Text))
                {
                    if (Directory.Exists(txtOutputFile.Text))
                    {
                        folderDialog.SelectedPath = txtOutputFile.Text;
                    }
                    else if (Directory.Exists(Path.GetDirectoryName(txtOutputFile.Text)))
                    {
                        folderDialog.SelectedPath = Path.GetDirectoryName(txtOutputFile.Text);
                    }
                }

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFile.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnBrowseCustomPrompt_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                openFileDialog.Title = "Chọn file prompt tùy chỉnh";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _customPromptPath = openFileDialog.FileName;
                    txtCustomPrompt.Text = _customPromptPath;
                }
            }
        }

        private void btnDarkMode_Click(object sender, EventArgs e)
        {
            ToggleDarkMode();
            btnDarkMode.Text = _isDarkMode ? "☀️ Light" : "🌙 Dark";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _shouldStop = true;
            AddLog("Đã yêu cầu dừng quá trình dịch...");
        }

    }
}