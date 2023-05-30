using System.Windows;
using Microsoft.Win32;

namespace IPRD_Checker
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            TxtUserAgent.Text = RegistryStore.UserAgent;
            TxtVlcLocation.Text = RegistryStore.VlcLocation.Trim();
            ComboTimeout.SelectedValue = RegistryStore.Timeout;
            ComboTries.SelectedValue = RegistryStore.NumTries;
            ComboThreads.SelectedValue = RegistryStore.NumThreads;
        }
        
        private void Btn_default_Click(object sender, RoutedEventArgs e)
        {
            TxtUserAgent.Text = "Mozilla/5.0 (Windows NT 6.2; Win64; x64;) Gecko/20100101 Firefox/126.0";
        }

        private void Btn_VLC_Location_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "VLC.EXE | *.exe"
            };
            openFileDialog.ShowDialog();
            var flag = openFileDialog.FileName != string.Empty;
            if (flag)
            {
                TxtVlcLocation.Text = openFileDialog.FileName;
            }
        }
        
        private void Btn_save_settings_Click(object sender, RoutedEventArgs e)
        {
            RegistryStore.UserAgent = TxtUserAgent.Text.Trim();
            RegistryStore.VlcLocation = TxtVlcLocation.Text.Trim();
            RegistryStore.NumTries = int.Parse(ComboTries.SelectedValue.ToString());
            RegistryStore.Timeout = int.Parse(ComboTimeout.SelectedValue.ToString());
            RegistryStore.NumThreads = int.Parse(ComboThreads.SelectedValue.ToString());
            RegistryStore.SaveToRegistry();
            Close();
        }
        
        private readonly RegistryStore RegistryStore = new RegistryStore();
    }
}
