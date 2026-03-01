using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpecTrace.Models;
using SpecTrace.Detectors;

namespace SpecTrace.Core
{
    public class SystemScanner
    {
        private readonly List<IDetector> _detectors;

        public SystemScanner()
        {
            _detectors = new List<IDetector>
            {
                new CpuDetector(),
                new MemoryDetector(),
                new GraphicsDetector(),
                new StorageDetector(),
                new NetworkDetector(),
                new MachineDetector()
            };
        }

        public async Task<SystemInfo> ScanAsync(bool deepScan = false, bool redact = false)
        {
            var systemInfo = new SystemInfo
            {
                GeneratedAt = DateTime.UtcNow,
                Redacted = redact
            };

            var tasks = new List<Task>();

            // Run all detectors in parallel with timeout
            foreach (var detector in _detectors)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
                        await detector.DetectAsync(systemInfo, deepScan, redact, cts.Token);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue - graceful degradation
                        System.Diagnostics.Debug.WriteLine($"Detector {detector.GetType().Name} failed: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return systemInfo;
        }

        public async Task<SystemInfo> LoadFromFileAsync(string filePath)
        {
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            var systemInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<SystemInfo>(json);
            return systemInfo ?? new SystemInfo();
        }
    }
}
