using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SecVers_Debloat.UI.Pages
{
    /// <summary>
    /// Interaktionslogik für AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        private const string GITHUB_REPO = "https://github.com/bunbunconmeow/Win11Debloater";
        private const string GITHUB_ISSUES = "https://github.com/bunbunconmeow/Win11Debloater/issues";
        private const string DOCUMENTATION = "https://github.com/bunbunconmeow/Win11Debloater/wiki";
        private const string LICENSE = "https://github.com/bunbunconmeow/Win11Debloater/blob/main/LICENSE";
        public AboutPage()
        {
            InitializeComponent();
        }

        private void BtnGitHub_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(GITHUB_REPO);
        }

        private void BtnIssues_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(GITHUB_ISSUES);
        }

        private void BtnDocumentation_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(DOCUMENTATION);
        }

        private void BtnLicense_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(LICENSE);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Failed to open URL: {0}\n\nError: {1}", url, ex.Message),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
