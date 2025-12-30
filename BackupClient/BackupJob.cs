using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupClient
{
    internal class BackupJob
    {
        public List<string> Sources { get; set; }
        public List<string> Targets { get; set; }
        public string Timing { get; set; }
        public BackupRetention retention { get; set; }
        public BackupMethod Method { get; set; }

        public BackupJob(List<string> sources, List<string> targets, string timing, BackupRetention retention, BackupMethod method)
        {
            Sources = sources;
            Targets = targets;
            Timing = timing;
            this.retention = retention;
            Method = method;
        }
    }
}
