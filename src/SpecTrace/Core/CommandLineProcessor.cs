using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SpecTrace.Core;
using SpecTrace.Models;
using Newtonsoft.Json;

namespace SpecTrace.Core
{
    public class CommandLineProcessor
    {
        public async Task ProcessAsync(string[] args)
        {
            var options = ParseArgs(args);
            
            if (options.ShowHelp)
            {
                ShowHelp();
                return;
            }

            var scanner = new SystemScanner();
            var systemInfo = await scanner.ScanAsync(options.DeepScan, options.Redact, options.Sections);

            await ExportResults(systemInfo, options);
            
            // Exit with appropriate code
            Environment.Exit(systemInfo.Cpu.Vendor == "" ? 2 : 0); // Partial if CPU not detected
        }

        private CommandLineOptions ParseArgs(string[] args)
        {
            var options = new CommandLineOptions();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--help":
                    case "-h":
                        options.ShowHelp = true;
                        break;
                    case "--quick":
                        options.DeepScan = false;
                        break;
                    case "--deep":
                        options.DeepScan = true;
                        break;
                    case "--redact":
                        options.Redact = true;
                        break;
                    case "--json":
                        if (i + 1 < args.Length)
                        {
                            options.JsonOutput = args[++i];
                        }
                        break;
                    case "--html":
                        if (i + 1 < args.Length)
                        {
                            options.HtmlOutput = args[++i];
                        }
                        break;
                    case "--markdown":
                        if (i + 1 < args.Length)
                        {
                            options.MarkdownOutput = args[++i];
                        }
                        break;
                    case "--text":
                        if (i + 1 < args.Length)
                        {
                            options.TextOutput = args[++i];
                        }
                        break;
                    case "--select":
                        if (i + 1 < args.Length)
                        {
                            options.Sections = args[++i].Split(',').ToList();
                        }
                        break;
                }
            }

            return options;
        }

        private void ShowHelp()
        {
            Console.WriteLine("SpecTrace - Advanced System Information Tool");
            Console.WriteLine();
            Console.WriteLine("Usage: SpecTrace.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --quick              Perform quick scan (no admin required)");
            Console.WriteLine("  --deep               Perform deep scan (requires admin)");
            Console.WriteLine("  --redact             Hide personally identifiable information");
            Console.WriteLine("  --json <file>        Export to JSON file");
            Console.WriteLine("  --html <file>        Export to HTML file");
            Console.WriteLine("  --markdown <file>    Export to Markdown file");
            Console.WriteLine("  --text <file>        Export to DXDiag-style text file");
            Console.WriteLine("  --select <sections>  Limit to specific sections (comma-separated)");
            Console.WriteLine("                       Available: cpu,memory,graphics,storage,network,usb,audio,sensors,security,processes");
            Console.WriteLine("  --help, -h           Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  SpecTrace.exe --quick --json system.json");
            Console.WriteLine("  SpecTrace.exe --deep --redact --html report.html");
            Console.WriteLine("  SpecTrace.exe --select cpu,graphics,storage --markdown specs.md");
        }

        private async Task ExportResults(SystemInfo systemInfo, CommandLineOptions options)
        {
            var exporter = new DataExporter();

            if (!string.IsNullOrEmpty(options.JsonOutput))
            {
                var json = exporter.ToJson(systemInfo);
                await File.WriteAllTextAsync(options.JsonOutput, json);
                Console.WriteLine($"JSON exported to: {options.JsonOutput}");
            }

            if (!string.IsNullOrEmpty(options.HtmlOutput))
            {
                var html = exporter.ToHtml(systemInfo);
                await File.WriteAllTextAsync(options.HtmlOutput, html);
                Console.WriteLine($"HTML exported to: {options.HtmlOutput}");
            }

            if (!string.IsNullOrEmpty(options.MarkdownOutput))
            {
                var markdown = exporter.ToMarkdown(systemInfo);
                await File.WriteAllTextAsync(options.MarkdownOutput, markdown);
                Console.WriteLine($"Markdown exported to: {options.MarkdownOutput}");
            }

            if (!string.IsNullOrEmpty(options.TextOutput))
            {
                var text = exporter.ToDxDiagText(systemInfo);
                await File.WriteAllTextAsync(options.TextOutput, text);
                Console.WriteLine($"Text exported to: {options.TextOutput}");
            }

            // If no output specified, print summary to console
            if (string.IsNullOrEmpty(options.JsonOutput) && 
                string.IsNullOrEmpty(options.HtmlOutput) && 
                string.IsNullOrEmpty(options.MarkdownOutput) && 
                string.IsNullOrEmpty(options.TextOutput))
            {
                Console.WriteLine(exporter.ToForumSummary(systemInfo));
            }
        }
    }

    public class CommandLineOptions
    {
        public bool ShowHelp { get; set; }
        public bool DeepScan { get; set; }
        public bool Redact { get; set; }
        public string JsonOutput { get; set; } = "";
        public string HtmlOutput { get; set; } = "";
        public string MarkdownOutput { get; set; } = "";
        public string TextOutput { get; set; } = "";
        public System.Collections.Generic.List<string> Sections { get; set; } = new();
    }
}
