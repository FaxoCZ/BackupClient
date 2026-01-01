using Quartz;
using Quartz.Impl;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Quartz.Logging.OperationName;

namespace BackupClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Backup();
        }
        public static void Backup()
        {
            string dataPath = GetJsonPath();
            Console.Clear();

            string JSONcontent = File.ReadAllText(dataPath);

            JsonSerializerOptions serializeOptions = new JsonSerializerOptions();

            serializeOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            serializeOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            List<BackupJob> backupJobs = JsonSerializer.Deserialize<List<BackupJob>>(JSONcontent, serializeOptions)!;

            foreach (BackupJob job in backupJobs)
            {
                foreach (string target in job.Targets)
                {
                    Directory.CreateDirectory(target);
                }
            }
            int methodCounter = 0;
            int fullRetentionCounter = 0;
            int incrementalRetentionCounter = 0;
            while (true)
            {
                foreach (var job in backupJobs)
                {
                    if (fullRetentionCounter == backupJobs[0].Retention.Count && BackupMethod.full == job.Method)
                    {
                        fullRetentionCounter = 0;
                        FullRetention(backupJobs[0]);
                    }
                    if(backupJobs.Count > 1)
                    {
                        if (incrementalRetentionCounter == backupJobs[1].Retention.Count)
                        {
                            incrementalRetentionCounter = 0;
                            IncrementalRetention(backupJobs[1]);

                        }
                    }
                    
                }
                

                foreach (var job in backupJobs)
                {
                    if (methodCounter == job.Retention.Size + 1)
                    {
                        methodCounter = 0;
                    }
                    if (methodCounter == 0)
                    {
                        if (job.Method == BackupMethod.full)
                        {
                            methodCounter++;
                            fullRetentionCounter++;
                            FullBackup(job);
                        }
                    }
                    else
                        if (job.Method == BackupMethod.incremental)
                    {
                        methodCounter++;
                        incrementalRetentionCounter++;
                        IncrementalBackup(job);                    
                    }
                }
            }
        }
        public static string GetJsonPath()
        {
            string? parentDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName).FullName).FullName).FullName + "\\JSONConfigs";


            var jsonFiles = Directory.GetFiles(parentDir, "*.json");

            if (jsonFiles.Length == 0)
            {
                Console.WriteLine("No JSON file found, write path: ");
                return Console.ReadLine()!;
            }

            Console.WriteLine("JSON files found:");
            foreach (string file in jsonFiles)
            {
                Console.WriteLine(Path.GetFileName(file));
            }

            Console.Write("\nChoose file: ");
            string? fileSel = Console.ReadLine();
            
            return parentDir + "\\" + fileSel;

        }
        public static void FullBackup(BackupJob backup)
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = factory.GetScheduler().Result;

            scheduler.Start().Wait();

            IJobDetail job = JobBuilder
                .Create<FullQuartzJob>()
                .Build();

            job.JobDataMap["backup"] = backup;

            ITrigger trigger = TriggerBuilder
                .Create()
                .WithCronSchedule("0 " + backup.Timing)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger).Wait();

            var cronExp = new CronExpression("0 " + backup.Timing);
            var nextTime = cronExp.GetNextValidTimeAfter(DateTimeOffset.Now);

            Console.WriteLine("Next job: Full");
            Console.WriteLine($"Scheduled at: {nextTime?.LocalDateTime}");

            Task.Delay(-1).Wait();
        }
        public static void IncrementalBackup(BackupJob backup)
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = factory.GetScheduler().Result;

            scheduler.Start().Wait();

            IJobDetail job = JobBuilder
                .Create<IncrementalQuartzJob>()
                .Build();

            job.JobDataMap["backup"] = backup;

            ITrigger trigger = TriggerBuilder
                .Create()
                .WithCronSchedule("0 " + backup.Timing)
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger).Wait();

            var cronExp = new CronExpression("0 " + backup.Timing);
            var nextTime = cronExp.GetNextValidTimeAfter(DateTimeOffset.Now);

            Console.WriteLine("Next job: Incremental");
            Console.WriteLine($"Scheduled at: {nextTime?.LocalDateTime}");

            Task.Delay(-1).Wait();
        }
        public static void FullRetention(BackupJob backup)
        {

            foreach (string target in backup.Targets)
            {
                var directories = new DirectoryInfo(target).GetDirectories("Full_*")
                    .OrderByDescending(d => d.CreationTime).ToList();
                
                    directories[0].Delete(true);
                    directories.RemoveAt(0);               
            }
        }
        public static void IncrementalRetention(BackupJob backup)
        {
            foreach (string target in backup.Targets)
            {
                var directories = new DirectoryInfo(target).GetDirectories("Incremental_*")
                    .OrderByDescending(d => d.CreationTime).ToList();

                for (int i = 0; i < backup.Retention.Size; i++)
                {
                    directories[0].Delete(true);
                    directories.RemoveAt(0);
                }
            }
        }
    }
}
