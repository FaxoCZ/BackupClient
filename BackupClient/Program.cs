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


            string dataPath = @"C:\Users\faxou\Desktop\BackupProject\BackupClient\config.json";

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
            int methodCounter = 2;
        while (true)
        {
                foreach (var job in backupJobs)
                {
                    if (methodCounter == 4)
                    {
                        methodCounter = 0;
                    }
                    if (methodCounter == 0)
                    {
                        if (job.Method == BackupMethod.full)
                        {
                            methodCounter++;
                            FullBackup(job);
                        }
                    }
                    else
                        if (job.Method == BackupMethod.incremental)
                    {
                        methodCounter++;
                        IncrementalBackup(job);
                    }
                }
            }
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
    }
}
