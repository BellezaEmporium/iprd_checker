using System;
using System.IO;
using Microsoft.Win32;

namespace IPRD_Checker
{
    public class RegistryStore
    {
        
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/126.0";
        
        public string VlcLocation { get; set; }
        
        public int Timeout { get; set; } = 5;
        
        public int NumThreads { get; set; } = 5;
        
        public int NumTries { get; set; } = 5;
        
        public string LastDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        
        private static string GetFromRegistry(string name)
        {
            string text;
            try
            {
                text = Registry.GetValue(AppNameRegistry, name, string.Empty).ToString();
            }
            catch (Exception)
            {
                text = string.Empty;
            }
            return text;
        }
        
        public static void SetRegistry(string name, string value)
        {
            Registry.SetValue(AppNameRegistry, name, value);
        }
        
        public RegistryStore()
        {
            VlcLocation = Find_VLC();
            var flag = GetFromRegistry("User_Agent") != string.Empty;
            if (flag)
            {
                UserAgent = GetFromRegistry("User_Agent");
            }
            var flag2 = GetFromRegistry("VLC_Location") != string.Empty;
            if (flag2)
            {
                VlcLocation = GetFromRegistry("VLC_Location");
            }
            var flag3 = GetFromRegistry("Timeout") != string.Empty;
            if (flag3)
            {
                Timeout = Convert.ToInt32(GetFromRegistry("Timeout"));
            }
            var flag4 = GetFromRegistry("NumTries") != string.Empty;
            if (flag4)
            {
                NumTries = Convert.ToInt32(GetFromRegistry("NumTries"));
            }
            var flag5 = GetFromRegistry("NumThreads") != string.Empty;
            if (flag5)
            {
                NumThreads = Convert.ToInt32(GetFromRegistry("NumThreads"));
            }
            var flag6 = GetFromRegistry("LastDir") != string.Empty;
            if (flag6)
            {
                LastDir = GetFromRegistry("LastDir");
            }
        }
        
        private static string Find_VLC()
        {
            var flag = File.Exists("C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe");
            var text = flag ? "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe" : string.Empty;
            return text;
        }
        
        public void SaveToRegistry()
        {
            SetRegistry("User_Agent", UserAgent.Trim());
            SetRegistry("VLC_Location", VlcLocation.Trim());
            SetRegistry("Timeout", Timeout.ToString());
            SetRegistry("NumTries", NumTries.ToString());
            SetRegistry("NumThreads", NumThreads.ToString());
            SetRegistry("LastDir", LastDir.ToLower().Trim());
        }

        // Token: 0x0400002A RID: 42
        private const string AppNameRegistry = "HKEY_CURRENT_USER\\Software\\IPRD_Checker";
    }
}
