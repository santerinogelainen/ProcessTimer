using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ProcessTimer
{
    public partial class MainWindow : Window
    {

        public const int TimerIntervalMinutes = 1;
        public List<ProcessDetails> Details { get; set; }
        public Settings Settings { get; set; }
        public int ThisProcessId => Process.GetCurrentProcess()?.Id ?? 0;
        private DispatcherTimer Timer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Settings = GetSettings();

            var iconUri = new Uri("pack://application:,,,/Resources/Main.ico");
            Icon = BitmapFrame.Create(iconUri);

            NotifyIcon icon = new NotifyIcon
            {
                Icon = new Icon(System.Windows.Application.GetResourceStream(iconUri).Stream),
                Visible = true,
                Text = "Todays processes"
            };

            icon.DoubleClick += ShowWindow;
            StartLoop();
            Hide();
        }

        protected Settings GetSettings()
        {
            if (File.Exists("settings.json"))
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            else
                return Settings.GetDefaultSettings();
        }

        protected void StartLoop()
        {
            Details = new List<ProcessDetails>();
            Timer = new DispatcherTimer {
                Interval = TimeSpan.FromMinutes(TimerIntervalMinutes)
            };
            Timer.Tick += Loop;
            Timer.Start();
            Loop(null, null); // do one loop
        }

        protected void Loop(object sender, EventArgs e)
        {
            // get current open processes
            IEnumerable<ProcessDetails> processes = Process
                .GetProcesses()
                .Where(x => 
                    !string.IsNullOrEmpty(x.MainWindowTitle) && 
                    !Settings.ExcludedProcesses.Contains(x.ProcessName) &&
                    x.Id != ThisProcessId)
                .Select(x => new ProcessDetails(x));

            // end details that are not open anymore
            IEnumerable<ProcessDetails> ended = Details
                .Where(x => !x.EndTime.HasValue && !processes.Any(y => y.Id == x.Id));
            foreach (var detail in ended) detail.EndTime = DateTime.Now;

            // add new processes that have been started
            IEnumerable<ProcessDetails> started = processes
                .Where(x => !Details.Any(y => y.Id == x.Id));
            Details.AddRange(started);

            // replace file 
            Directory.CreateDirectory("History");
            List<string> lines = new List<string>
            {
                ProcessDetails.CSVTitle
            };
            lines.AddRange(Details.Select(x => x.CSV));
            File.WriteAllLines(Settings.HistoryFolder + "/" + DateTime.Now.ToString("yyyyMMdd") + "_" + ThisProcessId + ".csv", lines);

            // bind lists
            ProcessGrid.ItemsSource = Details.OrderBy(x => x.StartTime);
            ProcessGrid.Items.Refresh();
        }

        protected void EndLoop()
        {
            Timer.Stop();
        }

        protected void ShowWindow(object sender, EventArgs args)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized) Hide();
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBoxResult result = System
                .Windows
                .MessageBox
                .Show("Are you sure you want to close the app? It will not continue to monitor your processes. Processes are saved in the history folder.", "Warning", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                e.Cancel = true;

            base.OnClosing(e);
        }

    }
}
