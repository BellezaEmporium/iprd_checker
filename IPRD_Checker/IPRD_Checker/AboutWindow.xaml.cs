using System.Net.Http;
using System.Threading.Tasks;

namespace IPRD_Checker
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            CheckForUpdatesAsync();
        }
        
        private async Task CheckForUpdatesAsync()
        {
            using (var client = new HttpClient())
            {
                var text = await client.GetStringAsync(_url);
                var actualversion = text;
                actualversion = actualversion.Replace("\n", "").Replace("\r", "");
                if (actualversion == currentVersion)
                {
                    TbUpdateVerif.Text = "You're up to date.";
                    TbVersion.Text = "IPTV Checker version " + currentVersion;
                }
                else
                {
                    TbUpdateVerif.Text = "A new update has been released !\nVersion " + actualversion + " has been released.";
                    TbVersion.Text = "IPTV Checker version " + currentVersion;
                }
            }
        }
        
        private readonly string _url = "https://raw.githubusercontent.com/LaneSh4d0w/iprd_checker/main/VERSION.txt";
        
        public static readonly string currentVersion = "2.6";
    }
}
