using System.Windows;

namespace IPRD_Checker
{
    public partial class AddLinkWindow
    {
        public AddLinkWindow()
        {
            InitializeComponent();
        }
        
        private void Btn_clear_txt_Click(object sender, RoutedEventArgs e)
        {
            TxtInput.Text = string.Empty;
        }
        
        private void Btn_add_text_Click(object sender, RoutedEventArgs e)
        {
            Str = TxtInput.Text.Trim();
            Close();
        }
        
        public string Str = string.Empty;
    }
}
