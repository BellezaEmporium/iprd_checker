using System.Windows;

namespace IPRD_Checker
{
    public partial class AddTextWindow
    {
        public AddTextWindow()
        {
            InitializeComponent();
        }
        
        private void Btn_clear_txt_Click(object sender, RoutedEventArgs e)
        {
            TxtInput.Text = string.Empty;
        }
        
        private void Btn_add_text_Click(object sender, RoutedEventArgs e)
        {
            _str = TxtInput.Text.Trim();
            Close();
        }

        private string _str = string.Empty;
    }
}
