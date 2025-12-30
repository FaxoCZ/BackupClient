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
                Console.WriteLine(string.Join(", ", job.Sources));
                Console.WriteLine(string.Join(", ", job.Targets));
                Console.WriteLine(job.Timing);
                Console.WriteLine(job.Retention);
                Console.WriteLine(job.Method);

            }
            foreach (BackupJob job in backupJobs)
            {
                foreach (string target in job.Targets)
                {
                    Directory.CreateDirectory(target);
                }
            } 
                       
        } 
    }
}
