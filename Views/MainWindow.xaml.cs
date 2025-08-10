using MahnitolaList.Helpers;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MahnitolaList.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var tag = LocalizationService.CurrentLanguage;
            var item = LanguageSelector.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(i => (string)i.Tag == tag) ?? (ComboBoxItem)LanguageSelector.Items[0];
            LanguageSelector.SelectedItem = item;
        }

        private void BtnGoToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            new PlaylistWindow().Show();
            Close();
        }

        private void BtnGoToAudioConverter_Click(object sender, RoutedEventArgs e)
        {
            new AudioConverterWindow().Show();
            Close();
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is ComboBoxItem item && item.Tag is string lang)
                LocalizationService.SetLanguage(lang);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
