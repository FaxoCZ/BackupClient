using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BackupClient
{
    internal class FullQuartzJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            
            var backup = (BackupJob)context.JobDetail.JobDataMap["backup"];

            int counter = 0;
            string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            foreach (string source in backup.Sources)
            {
                
                foreach (string target in backup.Targets)
                {
                    Directory.CreateDirectory(target + "\\Full_" + counter + "_" + time);
                    counter++;
                }
            }

            await context.Scheduler.DeleteJob(context.JobDetail.Key);
        }
    }
}
