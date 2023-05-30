using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IPRD_Checker.Models;
using Microsoft.Win32;
using Binding = System.Windows.Data.Binding;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace IPRD_Checker
{
    public partial class MainWindow
    {
        private string bindingpath;

        private Core core = Core.Instance;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = _core;
            Loaded += MainWindow_Loaded;
            Datagrid.Sorting += Datagrid_Sorting;
            Title += AboutWindow.currentVersion;
            LoadFromRegistry();
            BtnReset.IsEnabled = false;
        }
        
        private void Btn_about_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }
        
        private void Btn_add_m3u8_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                FileName = "",
                Filter = "M3u files (*.m3u8; *.m3u) | *.m3u8; *.m3u",
                InitialDirectory = _core.LastDir
            };
            openFileDialog.ShowDialog();
            var flag = openFileDialog.FileName != string.Empty;
            if (flag)
            {
                var str = File.ReadAllText(openFileDialog.FileName);
                var channels = _core.Parse(str);
                _core.Add_streams(channels);
                var directoryName = Path.GetDirectoryName(openFileDialog.FileName);
                _core.LastDir = directoryName;
                RegistryStore.SetRegistry("LastDir", directoryName);
            }
            TxtSearch.Text = string.Empty;
            RadioAll.IsChecked = true;
            FilterResults();
            var flag2 = _core.Channel_Full.Count > 0;
            BtnReset.IsEnabled = flag2;
        }
        
        private async void Btn_add_link_Click(object sender, RoutedEventArgs e)
        {
            var addLinkWindow = new AddLinkWindow();
            addLinkWindow.ShowDialog();
            var flag = addLinkWindow.Str.Length != 0;
            if (flag)
            {
                var count = this._core.Channel_Full.Count;
                HttpClient client;
                using (client = new HttpClient())
                {
                    var text = await client.GetStringAsync(addLinkWindow.Str);
                    var channels = _core.Parse(text);
                    _core.Add_streams(channels);
                    TxtSearch.Text = string.Empty;
                    RadioAll.IsChecked = true;
                    FilterResults();
                    MessageBox.Show($"Loaded {_core.Channel_Full.Count - count} channels into the list.", "IPTV Checker", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
            BtnReset.IsEnabled = _core.Channel_Full.Count > 0;
        }
        
        private void Btn_caseSensitive_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _core.CaseSensitiveSearch = !_core.CaseSensitiveSearch;
            var flag = !string.IsNullOrWhiteSpace(TxtSearch.Text);
            if (flag)
            {
                FilterResults();
            }
        }
        
        private async void Btn_check_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.IsEnabled = false;
            _core.CheckStatus = CheckStatus.Checking;
            await _core.StarChecking();
            _core.CheckStatus = _core.CheckStatus == CheckStatus.Stopping ? CheckStatus.Stopped : CheckStatus.Finished;
            BtnReset.IsEnabled = true;
            MessageBox.Show("Finished checking links...", "Links Checking", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void Btn_reset_Click(object sender, RoutedEventArgs e)
        {
            _sortDirection = ListSortDirection.Ascending;
            _core.Reset();
            BtnReset.IsEnabled = false;
            _core.StatusBarText = "Ready..";
        }
        
        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            _core.Save();
        }
        
        private void Btn_settings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
            LoadFromRegistry();
        }
        
        private void Btn_stop_Click(object sender, RoutedEventArgs e)
        {
            _core.CheckStatus = CheckStatus.Stopping;
            _core.Stop();
        }
        
        private void Btn_up_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndexes = GetSelectedIndexes();
            if (selectedIndexes[0] == 0)
            {
                return;
            }
            foreach (var item in selectedIndexes)
            {
                core.Channels.Move(item, item - 1);
            }
        }
        
        private void Btn_down_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndexes = GetSelectedIndexes();
            var num = selectedIndexes[selectedIndexes.Count - 1];
            selectedIndexes.Reverse();
            if (num == core.Channels.Count - 1)
            {
                return;
            }
            foreach (var item in selectedIndexes)
            {
                core.Channels.Move(item, item + 1);
            }
        }

        private void Check_group_Checked(object sender, RoutedEventArgs e)
        {
            if (GroupColumn != null)
            {
                GroupColumn.Visibility = Visibility.Visible;
            }
        }

        private void Check_group_Unchecked(object sender, RoutedEventArgs e)
        {
            if (GroupColumn != null)
            {
                GroupColumn.Visibility = Visibility.Collapsed;
            }
        }

        private void Check_logo_Checked(object sender, RoutedEventArgs e)
        {
            if (LogoColumn != null)
            {
                LogoColumn.Visibility = Visibility.Visible;
            }
        }

        private void Check_logo_Unchecked(object sender, RoutedEventArgs e)
        {
            if (LogoColumn != null)
            {
                LogoColumn.Visibility = Visibility.Collapsed;
            }
        }

        private void Check_server_Checked(object sender, RoutedEventArgs e)
        {
            if (ServerColumn != null)
            {
                ServerColumn.Visibility = Visibility.Visible;
            }
        }

        private void Check_server_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ServerColumn != null)
            {
                ServerColumn.Visibility = Visibility.Collapsed;
            }
        }

        private void Datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Play_in_VLC();
        }
        
        private void Datagrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            _bindingpath = ((Binding)e.Column.ClipboardContentBinding).Path.Path;
            new Channel();
            var flag = e.Column.SortDirection != null;
            if (flag)
            {
                var flag2 = e.Column.SortDirection.Value == ListSortDirection.Ascending;
                if (flag2)
                {
                    _sortDirection = ListSortDirection.Descending;
                    Sort();
                }
                else
                {
                    _sortDirection = ListSortDirection.Ascending;
                    Sort();
                }
            }
            else
            {
                _sortDirection = ListSortDirection.Ascending;
                Sort();
            }
        }
        
        private void FilterResults()
        {
            var status = Status.All;
            if (RadioAll.IsChecked == true)
            {
                status = Status.All;
            }
            if (RadioOffline.IsChecked == true)
            {
                status = Status.Offline;
            }
            if (RadioOnline.IsChecked == true)
            {
                status = Status.Online;
            }
            if (RadioUnchecked.IsChecked == true)
            {
                status = Status.Unchecked;
            }
            var list = status == Status.All ? _core.Channel_Full : _core.Channel_Full.Where(w => w.Status == status).ToList();
            if (_bindingpath != "")
            {
                list = _sortDirection switch
                {
                    ListSortDirection.Ascending => list.OrderBy(w => OrderSource(w, _bindingpath)).ToList(),
                    ListSortDirection.Descending => list.OrderByDescending(w => OrderSource(w, _bindingpath)).ToList(),
                    _ => list
                };
            }
            if (TxtSearch.Text.Trim() == string.Empty)
            {
                _core.Channels.Clear();
                foreach (var item in list)
                {
                    _core.Channels.Add(item);
                }
            }
            else
            {
                _core.Channels.Clear();
                foreach (var item2 in !_core.CaseSensitiveSearch ? list.Where(w => w.Name.ToLower().Contains(TxtSearch.Text.ToLower())) : list.Where(w => w.Name.Contains(TxtSearch.Text)))
                {
                    _core.Channels.Add(item2);
                }
            }
            _core.ChannelsCount = _core.Channels.Count;
        }
        
        private List<int> GetSelectedIndexes()
        {
            var list = new List<int>();
            for (var i = 0; i < Datagrid.Items.Count; i++)
            {
                var flag = Datagrid.SelectedItems.Contains(Datagrid.Items[i]);
                if (flag)
                {
                    list.Add(i);
                }
            }
            return list;
        }
        
        private void LoadFromRegistry()
        {
            var registryStore = new RegistryStore();
            _core.VlcLocation = registryStore.VlcLocation;
            _core.UserAgent = registryStore.UserAgent;
            _core.NumTries = registryStore.NumTries;
            _core.TimeOut = registryStore.Timeout;
            _core.NumThreads = registryStore.NumThreads;
            _core.LastDir = registryStore.LastDir;
        }
        
        private static void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ServicePointManager.DefaultConnectionLimit = 500;
        }
        
        private void Menu_copy_list_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndexes = GetSelectedIndexes();
            var stringBuilder = new StringBuilder();
            foreach (var item in selectedIndexes)
            {
                stringBuilder.AppendLine("#EXTINF:-1," + _core.Channels[item].Name);
                stringBuilder.AppendLine(_core.Channels[item].Url);
            }
            Clipboard.SetText(stringBuilder.ToString().Trim());
            MessageBox.Show("Channels have been copied to clipboard.", "Copy Channels as", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
        
        private void Menu_copy_url_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndexes = GetSelectedIndexes();
            var stringBuilder = new StringBuilder();
            foreach (var item in selectedIndexes)
            {
                stringBuilder.AppendLine(_core.Channels[item].Url);
            }
            Clipboard.SetText(stringBuilder.ToString().Trim());
            MessageBox.Show("Links have been copied to clipboard.", "Copy Channels", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
        
        private void Menu_delete_Click(object sender, RoutedEventArgs e)
        {
            var list = (from Channel selectedItem in Datagrid.SelectedItems select selectedItem.Url).ToList();
            using (var enumerator2 = list.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    var x = enumerator2.Current;
                    var item = _core.Channels.First(w => w.Url == x);
                    _core.Channel_Full.Remove(item);
                }
            }
            FilterResults();
            _core.StatusBarText = $"{list.Count} channels have been deleted";
        }
        
        private void Menu_play_Click(object sender, RoutedEventArgs e)
        {
            Play_in_VLC();
        }
        
        private void Menu_rename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var channel = (Channel)Datagrid.SelectedItem;
                var renameWindow = new RenameWindow(channel.Name);
                renameWindow.ShowDialog();
                var flag = renameWindow.ChannelName.Trim() != string.Empty;
                if (!flag) return;
                channel.Name = renameWindow.ChannelName.Trim();
                MessageBox.Show("Stream renamed successfully..", "Radio stream rename", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch
            {
                // ignored
            }
        }

        private static object OrderSource(Channel b, string name)
        {
            var runtimeProperty = b.GetType().GetRuntimeProperty(name);
            var flag = runtimeProperty != null;
            var obj = flag ? runtimeProperty.GetValue(b) : null;
            return obj;
        }
        
        private void Play_in_VLC()
        {
            try
            {
                var flag = _core.VlcLocation != string.Empty;
                if (flag)
                {
                    var url = ((Channel)Datagrid.SelectedItem).Url;
                    Process.Start(_core.VlcLocation, url);
                }
                else
                {
                    MessageBox.Show("Please install VLC Player or use settings to locate VLC Location", "VLC Player", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch
            {
                // ignored
            }
        }
        
        private void Radio_all_Click(object sender, RoutedEventArgs e)
        {
            FilterResults();
        }
        
        private void Radio_offline_Click(object sender, RoutedEventArgs e)
        {
            FilterResults();
        }
        
        private void Radio_online_Click(object sender, RoutedEventArgs e)
        {
            FilterResults();
        }
        
        private void Radio_unchecked_Click(object sender, RoutedEventArgs e)
        {
            FilterResults();
        }
        
        private void Sort()
        {
            var list = _sortDirection != ListSortDirection.Ascending ? _core.Channels.OrderByDescending(w => OrderSource(w, _bindingpath)).ToList() : _core.Channels.OrderBy(w => OrderSource(w, _bindingpath)).ToList();
            _core.Channels.Clear();
            foreach (var item in list)
            {
                _core.Channels.Add(item);
            }
        }
        
        private void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterResults();
        }

        private string _bindingpath = "";

        private readonly Core _core = Core.Instance;

        private ListSortDirection _sortDirection;
        
    }
}
