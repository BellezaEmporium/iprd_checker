using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IPRD_Checker.Models;
using Microsoft.Win32;

namespace IPRD_Checker
{
    public class Core : INotifyPropertyChanged
    {
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public bool CaseSensitiveSearch
        {
            get => caseSensitiveSearch;
            set
            {
                caseSensitiveSearch = value;
                OnPropertyChanged(nameof(CaseSensitiveSearch));
            }
        }

        public string StatusBarText
        {
            get => statusBarText;
            set
            {
                statusBarText = value;
                OnPropertyChanged(nameof(StatusBarText));
            }
        }

        public int CheckedPercentage
        {
            get => _checked_Percentage;
            set
            {
                _checked_Percentage = value;
                OnPropertyChanged(nameof(CheckedPercentage));
            }
        }

        public CheckStatus CheckStatus
        {
            get => _checkstatus;
            set
            {
                _checkstatus = value;
                OnPropertyChanged(nameof(CheckStatus));
            }
        }

        public ObservableCollection<Channel> Channels
        {
            get;
            set;
        }

        public List<Channel> Channel_Full
        {
            get;
            set;
        }

        public int ChannelsCount
        {
            get => _channelsCount;
            set
            {
                _channelsCount = value;
                OnPropertyChanged(nameof(ChannelsCount));
            }
        }

        public int Online_count
        {
            get => _online_count;
            set
            {
                _online_count = value;
                OnPropertyChanged(nameof(Online_count));
            }
        }

        public int Offline_count
        {
            get => _offline_count;
            set
            {
                _offline_count = value;
                OnPropertyChanged(nameof(Offline_count));
            }
        }

        public string Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                OnPropertyChanged(nameof(Checked));
            }
        }

        public string UserAgent
        {
            get;
            set;
        }

        public string VlcLocation
        {
            get;
            set;
        }

        public int TimeOut
        {
            get;
            set;
        }

        public int NumThreads
        {
            get;
            set;
        }

        public int NumTries
        {
            get;
            set;
        }

        public string LastDir
        {
            get;
            set;
        }

        private string Country
        {
            get;
            set;
        }

        public static Core Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Core
                        {
                            Channel_Full = new List<Channel>(),
                            UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64;) Gecko/20100101 Firefox/20.0",
                            Channels = new ObservableCollection<Channel>(),
                            Checked = "∞",
                            StatusBarText = "Ready..",
                            Country = ""
                        };
                    }
                    return instance;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private Core()
        {
        }

        public List<Channel> Parse(string str)
        {
            statusBarText = "Importing streams..";
            var list = new List<Channel>();
            foreach (var obj in new Regex("#([^#]*)", RegexOptions.Singleline).Matches(str))
            {
                var item = (Match)obj;
                var text = item.Value.Trim();
                var text2 = Regex.Match(text, "#.*").Value.Trim();
                var text3 = text.Replace(text2, "").Trim();
                text2 = text2.Substring(text2.LastIndexOf(",", StringComparison.Ordinal) + 1);
                var flag = text3.Trim() != string.Empty;
                if (!flag) continue;
                if (text2 != null)
                    list.Add(new Channel
                    {
                        Url = text3,
                        Name = text2.Trim()
                    });
            }
            return list;
        }
        
        public void Add_streams(List<Channel> channels)
        {
            var flag = channels == null;
            if (flag)
            {
                StatusBarText = "No stream has been imported";
            }
            else
            {
                channels = channels.Where(w => w != null).ToList();
                var list = channels.Except(Channel_Full).ToList();
                var num = channels.Count - list.Count;
                Channel_Full.AddRange(list);
                StatusBarText = (num != 0) ? $"Found {channels.Count}, imported {list.Count} streams, and skipped {num} duplicates."
                    : $"Imported {list.Count()} streams.";
            }
        }
        
        public void Reset()
        {
            Channels.Clear();
            Channel_Full.Clear();
            Online_count = 0;
            ChannelsCount = 0;
            Offline_count = 0;
            Checked = "∞";
            CheckedPercentage = 0;
        }
        
        public async Task StarChecking()
        {
            cancellationToken = new CancellationTokenSource();
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = NumThreads,
                CancellationToken = cancellationToken.Token
            };
            var sourceTobeChecked = Channels.Where(w => w.Status == Status.Unchecked).ToList();
            cancel_request = false;
            try
            {
                await Task.Run(delegate
                {
                    try
                    {
                        Parallel.ForEach(sourceTobeChecked, parallelOptions, async delegate (Channel channel)
                        {
                            try
                            {
                                channel.Status = await CheckUrl(channel.Url);
                                Online_count = sourceTobeChecked.Count(w => w.Status == Status.Online);
                                Offline_count = sourceTobeChecked.Count(w => w.Status == Status.Offline);
                                Checked = sourceTobeChecked.Count(w => w.Status == Status.Online || w.Status == Status.Offline) + "/" + sourceTobeChecked.Count;
                                CheckedPercentage = sourceTobeChecked.Count(w => w.Status == Status.Online || w.Status == Status.Offline) * 100 / sourceTobeChecked.Count;
                                cancellationToken.Token.ThrowIfCancellationRequested();
                            }
                            catch
                            {
                            }
                        });
                    }
                    catch
                    {
                    }
                }, cancellationToken.Token);
            }
            catch
            {
            }
        }
        
        private async Task<Status> CheckUrl(string url)
        {
            for (var i = 1; i <= NumTries; i++)
            {
                var flag = cancel_request;
                if (flag) continue;
                try
                {
                    var client = new HttpClient(); 
                    ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; 
                    using (var response = client.GetAsync(url).Result) 
                    { 
                        using (var responseContent = response.Content) 
                        { 
                            var data = await responseContent.ReadAsByteArrayAsync(); 
                            if (data != null && data.Length > 0) 
                            { 
                                return Status.Online; 
                            } 
                        } 
                    }
                }
                catch (WebException ex)
                {
                    var flag2 = ex.Response is HttpWebResponse httpWebResponse && httpWebResponse.StatusCode == HttpStatusCode.NotFound;
                    if (flag2)
                    {
                        return Status.Offline;
                    }
                    var flag3 = ex.Response is HttpWebResponse httpWebResponse2 && httpWebResponse2.StatusCode == HttpStatusCode.ServiceUnavailable;
                    if (flag3)
                    {
                        return Status.Offline;
                    }
                    _ = ex.Response is HttpWebResponse httpWebResponse3 && httpWebResponse3.StatusCode == HttpStatusCode.BadGateway;
                    return Status.Offline;
                }
            }
            Online_count = Channels.Count(w => w.Status == Status.Online);
            Offline_count = Channels.Count(w => w.Status == Status.Offline);
            Checked = Channels.Count(w => w.Status == Status.Online || w.Status == Status.Offline) + "/" + Channels.Count;
            return Status.Offline;
        }
        
        public void Stop()
        {
            cancel_request = true;
            cancellationToken.Cancel();
        }
        
        public void Save()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Playlist file (*.m3u, *.m3u8) | *.m3u, *.m3u8;",
                FileName = "",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            saveFileDialog.ShowDialog();
            var flag = saveFileDialog.FileName != string.Empty;
            if (flag)
            {
                var fileName = saveFileDialog.FileName;
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("#EXTM3U");
                foreach (var channel in Channels)
                {
                    stringBuilder.AppendLine("#EXTINF:-1," + channel.Name);
                    stringBuilder.AppendLine(channel.Url);
                }
                File.WriteAllText(fileName, stringBuilder.ToString().Trim());
                MessageBox.Show("File has been saved successfully", "File Save", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                MessageBox.Show("File save has been cancelled", "File Save", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        
        private static Core instance;
        
        private static readonly object padlock = new object();
        
        private CancellationTokenSource cancellationToken;
        
        private bool cancel_request;
        
        private int _online_count;
        
        private int _offline_count;
        
        private string _checked;
        
        private int _channelsCount;
        
        private int _checked_Percentage;
        
        private bool caseSensitiveSearch;
        
        private CheckStatus _checkstatus;
        
        private string statusBarText;

        private bool isBusy;
    }
}
