using System.Windows;

namespace IPRD_Checker
{
    public partial class RenameWindow
    {
        public string ChannelName { get; private set; }
        
        public RenameWindow(string channelName)
        {
            InitializeComponent();
            ChannelName = channelName;
            TxtChannelName.Text = channelName;
        }
        
        private void Btn_ok_Click(object sender, RoutedEventArgs e)
        {
            ChannelName = TxtChannelName.Text.Trim();
            Close();
        }
    }
}
