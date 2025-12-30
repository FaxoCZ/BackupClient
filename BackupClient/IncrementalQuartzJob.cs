using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupClient
{
    internal class IncrementalQuartzJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {

            var backup = (BackupJob)context.JobDetail.JobDataMap["backup"];

            string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            Directory.CreateDirectory(backup.Targets + "\\Incremental_" + time);

            List<string> sourceDir = backup.Sources;
            string fullBackupDir = backup.Targets[0] + "\\Full";
            string incrementalDir = backup.Targets + "\\Incremental_" + time;

            foreach (string sourceDirs in sourceDir)
            {
                string sourceRootName = new DirectoryInfo(sourceDirs).Name;

                foreach (string sourceFile in Directory.GetFiles(
                             sourceDirs, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(sourceDirs, sourceFile);

                    string fullFile = Path.Combine(
                        fullBackupDir, sourceRootName, relativePath);

                    string incFile = Path.Combine(
                        incrementalDir, sourceRootName, relativePath);

                    bool Copy = false;

                    if (!File.Exists(fullFile))
                    {
                        Copy = true;
                    }
                    else
                    {
                        DateTime sourceTime = File.GetLastWriteTime(sourceFile);
                        DateTime fullTime = File.GetLastWriteTime(fullFile);

                        if (sourceTime > fullTime)
                            Copy = true;
                    }

                    if (Copy)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(incFile)!);
                        File.Copy(sourceFile, incFile, true);
                    }
                }
            }
            Console.Clear();
            Console.WriteLine("Incremental backup done succesfully");
            Thread.Sleep(2000);
            await context.Scheduler.DeleteJob(context.JobDetail.Key);
        }
    }
}
