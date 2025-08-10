using MahnitolaList.Helpers;
using MahnitolaList.Models;
using MahnitolaList.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace MahnitolaList.Views
{
    public partial class AudioConverterWindow : Window
    {
        private readonly AudioConvertService _converter = new();
        public ObservableCollection<ConversionItem> Items { get; } = new();

        private string _outputDir = string.Empty;

        public AudioConverterWindow()
        {
            InitializeComponent();
            FilesList.ItemsSource = Items;
        }

        private void BtnPickFiles_Click(object sender, RoutedEventArgs e)
        {
            using var ofd = new WinForms.OpenFileDialog
            {
                Filter = "Audio (*.m4a;*.mp3)|*.m4a;*.mp3",
                Multiselect = true
            };
            if (ofd.ShowDialog() == WinForms.DialogResult.OK)
            {
                AddFiles(ofd.FileNames);
            }
        }

        private void FilesList_OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                var paths = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                var audio = paths.Where(HasSupportedExt);
                AddFiles(audio);
            }
        }

        private static bool HasSupportedExt(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".mp3" or ".m4a";
        }

        private void AddFiles(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (!File.Exists(path)) continue;

                var suggested = FileNameSanitizer.SuggestOutputName(
                    Path.GetFileNameWithoutExtension(path));

                Items.Add(new ConversionItem
                {
                    InputPath = path,
                    InputName = Path.GetFileName(path),
                    OutputName = suggested + ".wav"
                });
            }
        }

        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            Items.Clear();
        }

        private void BtnChooseOutput_Click(object sender, RoutedEventArgs e)
        {
            using var fbd = new WinForms.FolderBrowserDialog();
            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                _outputDir = fbd.SelectedPath;
            }
        }

        private async void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (!Items.Any())
            {
                MessageBox.Show(
                    FindResource("AudioConv_Err_NoFiles")?.ToString() ?? "No files selected.",
                    FindResource("Msg_Error")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_outputDir))
            {
                MessageBox.Show(
                    FindResource("AudioConv_Err_NoOutput")?.ToString() ?? "Choose output folder.",
                    FindResource("Msg_Error")?.ToString() ?? "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnConvert.IsEnabled = false;

            foreach (var item in Items)
            {
                try
                {
                    var cleanFile = FileNameSanitizer.CleanFileName(
                        Path.GetFileNameWithoutExtension(item.OutputName));

                    var outPath = Path.Combine(_outputDir, cleanFile + ".wav");

                    if (!ChkReplaceExisting.IsChecked.GetValueOrDefault(false) && File.Exists(outPath))
                        continue;

                    await _converter.ConvertToWav20kAsync(item.InputPath, outPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(FindResource("AudioConv_Err_Item")?.ToString() ?? "Error converting \"{0}\": {1}", item.InputName, ex.Message),
                        FindResource("Msg_Error")?.ToString() ?? "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            BtnConvert.IsEnabled = true;

            MessageBox.Show(
                FindResource("AudioConv_Done")?.ToString() ?? "Conversion completed successfully.",
                FindResource("Msg_Done")?.ToString() ?? "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
