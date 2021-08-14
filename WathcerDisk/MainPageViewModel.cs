using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using FireSharp.Response;
using FireSharp.Interfaces;
using FireSharp.Config;
using System.Threading;
using System.IO;
using GalaSoft.MvvmLight.CommandWpf;

namespace WathcerDisk
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        Logger logger;
        IFirebaseConfig config = new FirebaseConfig()
        {
            AuthSecret = "JuN06Hm78gQ5sMRoNPjUDHOiprp7B3Jq1GVxYY2h",
            BasePath = "https://watcherservice-1810a-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        IFirebaseClient client;


        public MainPageViewModel()
        {     
            logger = new Logger();
            Thread loggerThread = new Thread(new ThreadStart(logger.Start));
            loggerThread.Start();
            Task.Run(() => TrySelect());
        }

 


       public void TrySelect()
       {
                try
                {
                    client = new FireSharp.FirebaseClient(config);
                }
                catch
                {
                    MessageBox.Show("Ошибка с подключением к интернету!");
                }
                ObservableCollection<Data> TempDataColl = new ObservableCollection<Data>();
                for (int i = 0; i < 31; i++)
                {
                       var result = client?.Get("DatasList/"+ i);
                       Data TempData = result.ResultAs<Data>();
                       if (TempData != null)
                       {
                        TempDataColl.Add(TempData);
                       }
                }
            Datas = TempDataColl;
                
        }

        public ObservableCollection<Data> _data;

        public ObservableCollection<Data> Datas
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        class Logger
        {
            IFirebaseConfig config = new FirebaseConfig()
            {
                AuthSecret = "JuN06Hm78gQ5sMRoNPjUDHOiprp7B3Jq1GVxYY2h",
                BasePath = "https://watcherservice-1810a-default-rtdb.europe-west1.firebasedatabase.app/"
            };
            IFirebaseClient client;
            FileSystemWatcher watcher;
            object obj = new object();
            bool enabled = true;
            public Logger()
            {
                watcher = new FileSystemWatcher("C:\\Users\\User\\Downloads");
                watcher.Deleted += Watcher_Deleted;
                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
            }

            public void Start()
            {
                watcher.EnableRaisingEvents = true;
                while (enabled)
                {
                    Thread.Sleep(1000);
                }
            }
            public void Stop()
            {
                watcher.EnableRaisingEvents = false;
                enabled = false;
            }
            // переименование файлов
            private void Watcher_Renamed(object sender, RenamedEventArgs e)
            {
                string fileEvent = "переименован в " + e.FullPath;
                string filePath = e.OldFullPath;
                RecordEntry(fileEvent, filePath);
            }
            // изменение файлов
            private void Watcher_Changed(object sender, FileSystemEventArgs e)
            {
                string fileEvent = "изменен";
                string filePath = e.FullPath;
                RecordEntry(fileEvent, filePath);
            }
            // создание файлов
            private void Watcher_Created(object sender, FileSystemEventArgs e)
            {
                string fileEvent = "создан";
                string filePath = e.FullPath;
                RecordEntry(fileEvent, filePath);
            }
            // удаление файлов
            private void Watcher_Deleted(object sender, FileSystemEventArgs e)
            {
                string fileEvent = "удален";
                string filePath = e.FullPath;
                RecordEntry(fileEvent, filePath);
            }

            private void RecordEntry(string fileEvent, string filePath)
            {
                lock (obj)
                {

                    Data dat = new Data() { Work = String.Format("{0} файл {1} был {2}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent) };                            
                    try
                    {
                        client = new FireSharp.FirebaseClient(config);
                    }
                    catch
                    {
                        MessageBox.Show("Error: Network");
                    }
                    int per = Int32.Parse(File.ReadAllText("save.txt", Encoding.Default));                 
                    var setter = client.Set("DatasList/" + per, dat);
                    per = per + 1;
                    
                    if (per <= 30)
                    {
                        File.WriteAllText("save.txt", per.ToString());
                    }
                    else
                    {
                        var r = client.DeleteAsync("DatasList/");
                        File.WriteAllText("save.txt", 0.ToString());
                    }
                }

            }
        }
        /// <summary>
        /// /////////////////////////////////////////////////////
        /// </summary>
        private ICommand _myCommand;
        public ICommand MyCommand
        {
            get
            {
                if (_myCommand == null)
                { _myCommand = new RelayCommand<object>(this.MyCommand_Execute); }
                return _myCommand;
            }
        }

        private async void MyCommand_Execute(object parameter)
        {
            await Task.Run(() => TrySelect());
        }

        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////
        /// </summary>


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        




    }
}
