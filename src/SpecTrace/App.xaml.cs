using System;
using System.Windows;
using System.Threading.Tasks;
using SpecTrace.Views;
using System.Linq;

namespace SpecTrace
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Handle command line arguments
            var args = e.Args;
            if (args.Length > 0)
            {
                HandleCommandLine(args);
                return;
            }

            // Start GUI application
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void HandleCommandLine(string[] args)
        {
            var cliProcessor = new Core.CommandLineProcessor();
            Task.Run(async () =>
            {
                try
                {
                    await cliProcessor.ProcessAsync(args);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                    Environment.Exit(3);
                }
                finally
                {
                    Environment.Exit(0);
                }
            }).Wait();
        }
    }
}
