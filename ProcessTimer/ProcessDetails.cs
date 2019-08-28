using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessTimer
{
    public class ProcessDetails
    {

        public const string CSVTitle = "Process;Window;Start time;End time;Uptime";

        public int Id => Process.Id;
        public string WindowName => Process.MainWindowTitle;
        public string ProcessName => Process.ProcessName;
        public DateTime StartTime => Process.StartTime;
        public DateTime? EndTime { get; set; }
        public TimeSpan Uptime => ((EndTime ?? DateTime.Now) - StartTime);

        #region Visual

        public string FormattedStartTime => StartTime.ToString("HH:mm");
        public string FormattedEndTime => EndTime?.ToString("HH:mm");
        public string FormattedUptime => Uptime.ToString(@"hh\:mm");
        public string CSV => ProcessName + ";" + 
            WindowName + ";" + 
            FormattedStartTime + ";" + 
            FormattedEndTime + ";" + 
            FormattedUptime;

        #endregion

        public Process Process { get; set; }

        public ProcessDetails(Process process)
        {
            Process = process;
        }

    }
}
