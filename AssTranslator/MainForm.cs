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
        private string _selectedModel = "gemini-2.0-flash";
        private string _inputFilePath = "";
        private string _outputFilePath = "";
        private string _customPromptPath = "";
        private bool _isTranslating = false;
        private string _selectedLanguage = "Vietnamese";
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
            "gemini-2.0-pro"
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
        }

        private void SaveSettings()
        {
            // Save API key to environment variable
            Environment.SetEnvironmentVariable("GOOGLE_API_KEY", _apiKey, EnvironmentVariableTarget.User);
            
            // Save selected language
            Properties.Settings.Default.Language = _selectedLanguage;
            Properties.Settings.Default.Save();
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

            if (string.IsNullOrEmpty(_inputFilePath))
            {
                MessageBox.Show("Vui lòng chọn file phụ đề cần dịch.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_outputFilePath))
            {
                MessageBox.Show("Vui lòng chọn nơi lưu file phụ đề đã dịch.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _isTranslating = true;
                btnTranslate.Enabled = false;
                progressBar.Visible = true;
                lblStatus.Visible = true;

                var translator = new AssTranslator(_apiKey, _selectedModel, _customPromptPath, _selectedLanguage);
                translator.ProgressChanged += Translator_ProgressChanged;
                translator.StatusChanged += Translator_StatusChanged;

                bool success = await translator.TranslateFile(_inputFilePath, _outputFilePath);

                if (success)
                {
                    MessageBox.Show("Dịch phụ đề thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    txtInputFile.Text = _inputFilePath;
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
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "ASS Files (*.ass)|*.ass|All Files (*.*)|*.*";
                saveFileDialog.Title = "Chọn nơi lưu file phụ đề đã dịch";
                
                if (!string.IsNullOrEmpty(_outputFilePath))
                {
                    saveFileDialog.FileName = Path.GetFileName(_outputFilePath);
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(_outputFilePath);
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _outputFilePath = saveFileDialog.FileName;
                    txtOutputFile.Text = _outputFilePath;
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
    }
}