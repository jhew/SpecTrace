using System.Threading;
using System.Threading.Tasks;
using SpecTrace.Models;

namespace SpecTrace.Detectors
{
    public interface IDetector
    {
        Task DetectAsync(SystemInfo systemInfo, bool deepScan, bool redact, CancellationToken cancellationToken);
    }
}
