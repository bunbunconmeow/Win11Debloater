using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using SecVers_Debloat.Extensions;
using SecVers_Debloat.Network;
using System.Windows.Media.Animation;

namespace SecVers_Debloat
{
    public partial class MainWindow : Window
    {
        UI.Popup.Telemetry_Popup popup = new UI.Popup.Telemetry_Popup();

        public MainWindow()
        {
            InitializeComponent();
            NavigateToPage("WelcomePage");
            popup.TelemetryChoiceMade += OnTelemetryChoiceMade;
            popup.ShowDialog();

          
        }

        private void OnTelemetryChoiceMade(object sender, TelemetryChoiceEventArgs e)
        {
          
        }

      

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Navigate to Settings Page
                NavigateToPage("SettingsPage");
            }
            else if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                string pageTag = selectedItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(pageTag))
                {
                    NavigateToPage(pageTag);
                }
            }
        }

        private void NavigateToPage(string pageTag)
        {
            Type pageType = null;

            switch (pageTag)
            {
                case "WelcomePage":
                    pageType = typeof(UI.Pages.WelcomePage);
                    break;
                case "DebloatPage":
                    pageType = typeof(UI.Pages.DebloatPage);
                    break;
                case "SoftwareInstallerPage":
                    pageType = typeof(UI.Pages.SoftwareInstallerPage);
                    break;
                case "CommunityScriptsPage":
                    pageType = typeof(UI.Pages.CommunityScriptsPage);
                    break;
                case "DefenderPage":
                    pageType = typeof(UI.Pages.DefenderPage);
                    break;
                case "HardeningPage":
                    pageType = typeof(UI.Pages.HardeningPage);
                    break;
                case "AboutPage":
                    pageType = typeof(UI.Pages.AboutPage);
                    break;
                case "RegistryTweaks":
                    pageType = typeof(UI.Pages.RegistryTweaksPage);
                    break;
                case "AnonymizerPage":
                    pageType = typeof(UI.Pages.AnonymizerPage);
                    break;
            }

            if (pageType != null)
            {
                ContentFrame.Navigate(Activator.CreateInstance(pageType));
            }
        }
    }
}
