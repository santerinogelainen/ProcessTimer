using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessTimer
{
    public class Settings
    {

        public string[] ExcludedProcesses { get; set; }
        public string HistoryFolder { get; set; }



        public static Settings GetDefaultSettings()
        {
            return new Settings
            {
                ExcludedProcesses = new string[0],
                HistoryFolder = "History"
            };
        }

    }
}
