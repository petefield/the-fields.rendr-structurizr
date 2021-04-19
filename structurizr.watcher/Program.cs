using System;
using System.IO;
using System.Security.Cryptography;
using System.Management.Automation.Runspaces;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using Microsoft.PowerShell;
using System.Management.Automation;

namespace thefields.structurizrwatcher
{
    class Program
    {
        private static FileSystemWatcher fsw;
        private static string prevHash;
        private static HubConnection connection;
        private static string pathToWatch;
        private static string fileToWatch;
        private static bool reloadEnabled = true;

        static async Task Main(string[] args)
        {
            if (args.Length ==0 || String.IsNullOrWhiteSpace(args[0])) throw new ArgumentException("Path to a file must be supplied as the first command line argument.");
            if (!System.IO.File.Exists(args[0])) throw new FileNotFoundException(args[0]);
            fileToWatch = System.IO.Path.GetFileName(args[0]);
            pathToWatch = System.IO.Path.GetDirectoryName(args[0]);

            fsw = new System.IO.FileSystemWatcher(pathToWatch);
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.Changed += OnChanged;
            fsw.Filter = fileToWatch;

            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/ChatHub")
                    .Build();

                connection.Closed += async (error) => {
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await connection.StartAsync();
                };

                await connection.StartAsync();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                reloadEnabled = false;
            }
            fsw.EnableRaisingEvents = true;

            Console.WriteLine("Watching... Press enter to exit.");
            Console.ReadLine();
        }

        private async static void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                fsw.EnableRaisingEvents = false;
                if (e.ChangeType != WatcherChangeTypes.Changed) return;
                
                using var stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                if (stream.Length == 0) return;

                var hash = CalculateMD5(stream);
                if (hash != prevHash)
                {
                    prevHash = hash;
                    Console.Write($"File {e.FullPath} has been updated.  Rendering Image...");
                    RunScript(e.FullPath);

                    File.Copy(@"temp.png", System.IO.Path.Combine(Path.GetTempPath(), "the-fields.rendr-structurizr", "temp.png") , true);

                    Console.WriteLine("Done.");
                    if (reloadEnabled)                    {
                        await connection.InvokeAsync("Refresh");
                    }
                }
            }
            finally
            {
                fsw.EnableRaisingEvents = true;
            }
        }

        static string CalculateMD5(Stream stream)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static void RunScript(string path)
        {
            InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;
            File.Copy(@path, @"temp.dsl", true);

            using (Runspace runspace = RunspaceFactory.CreateRunspace(initialSessionState))
            {
                runspace.Open();
                using (Pipeline pipeline = runspace.CreatePipeline())
                {
                    pipeline.Commands.Add(@"scripts\run.ps1");
                    pipeline.Invoke();
                }
                runspace.Close();
            }
        }
    }
}
