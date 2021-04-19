using System;
using System.IO;
using System.Security.Cryptography;
using System.Management.Automation.Runspaces;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;



using Microsoft.PowerShell;
using System.Management.Automation;
using System.Management.Automation.Runspaces;


namespace Test
{
    class Program
    {
        private static FileSystemWatcher fsw = new System.IO.FileSystemWatcher(@"C:\Users\peter578\Desktop\Test");
        private static string prevHash;
        private static HubConnection connection;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.Changed += OnChanged;
            fsw.Filter = "test.dsl";

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/ChatHub")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                await connection.StartAsync();
            };

            await connection.StartAsync();

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
                    RunScript();

                    File.Copy(@"C:\Users\peter578\Desktop\Test\test.png", @"C:\Users\peter578\Desktop\Test\web\wwwroot\test.png", true);

                    Console.WriteLine("Done.");
                    await connection.InvokeAsync("Refresh");
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

        private static void RunScript()
        {
                      InitialSessionState initialSessionState = InitialSessionState.CreateDefault();

                      initialSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;

            using (Runspace runspace = RunspaceFactory.CreateRunspace(initialSessionState))
            {
                runspace.Open();
                runspace.SessionStateProxy.Path.SetLocation(@"C:\Users\peter578\Desktop\Test");
                using (Pipeline pipeline = runspace.CreatePipeline())
                {
                    pipeline.Commands.Add(@"C:\Users\peter578\Desktop\Test\run.ps1");
                    pipeline.Invoke();
                }
                runspace.Close();
            }
        }
    }
}
