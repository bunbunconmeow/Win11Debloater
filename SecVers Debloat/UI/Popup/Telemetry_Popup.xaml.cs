using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SecVers_Debloat.Cache.Popup;
using static SecVers_Debloat.Cache.Global;
using SecVers_Debloat.Helper;
using SecVers_Debloat.Cache;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using SecVers_Debloat.Extensions;

namespace SecVers_Debloat.UI.Popup
{
    /// <summary>
    /// Interaktionslogik für Telemetry_Popup.xaml
    /// </summary>
    public partial class Telemetry_Popup : Window
    {
        public event EventHandler<TelemetryChoiceEventArgs> TelemetryChoiceMade;

        public Telemetry_Popup()
        {
            InitializeComponent();
        }
        private void BtnAllow_Click(object sender, RoutedEventArgs e)
        {
            Cache.Popup.Set_AllowTelemetry(true);

            MessageBox.Show("Thank you! Telemetry has been enabled.",
                          "Telemetry Enabled",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

            TelemetryChoiceMade?.Invoke(this, new TelemetryChoiceEventArgs(true));
            this.DialogResult = true;
            this.Close();
        }

        private void BtnDecline_Click(object sender, RoutedEventArgs e)
        {
            Cache.Popup.Set_AllowTelemetry(false);
            TelemetryChoiceMade?.Invoke(this, new TelemetryChoiceEventArgs(false));

            this.DialogResult = false;
            this.Close();
        }
    }
}
