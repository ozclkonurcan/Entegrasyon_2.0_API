using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace moduleTest.Controllers
{

    //Burası ileride cqrs ile ayrılabilir test amaçlı yer alıyor şimdilik
    [Route("api/[controller]")]
    [ApiController]
    public class PerformanceController : ControllerBase
    {
        private readonly IPerformanceService _performanceService;

        public PerformanceController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        /// <summary>
        /// Anlık performans bilgilerini getirir
        /// </summary>
        [HttpGet("current")]
        public ActionResult<PerformanceMetrics> GetCurrentMetrics()
        {
            var metrics = _performanceService.GetCurrentMetrics();
            return Ok(metrics);
        }

        /// <summary>
        /// Detaylı sistem bilgilerini getirir
        /// </summary>
        [HttpGet("system-info")]
        public ActionResult<SystemInfo> GetSystemInfo()
        {
            var systemInfo = _performanceService.GetSystemInfo();
            return Ok(systemInfo);
        }

        /// <summary>
        /// Belirli bir süre boyunca performans verilerini izler
        /// </summary>
        [HttpGet("monitor")]
        public async Task<ActionResult<List<PerformanceSnapshot>>> MonitorPerformance(
            [FromQuery] int durationSeconds = 10,
            [FromQuery] int intervalSeconds = 1)
        {
            if (durationSeconds > 60 || durationSeconds < 1)
                return BadRequest("Süre 1-60 saniye arasında olmalıdır");

            if (intervalSeconds < 1)
                return BadRequest("Interval en az 1 saniye olmalıdır");

            var snapshots = await _performanceService.MonitorAsync(durationSeconds, intervalSeconds);
            return Ok(snapshots);
        }

        /// <summary>
        /// Disk kullanım bilgilerini getirir
        /// </summary>
        [HttpGet("disk-usage")]
        public ActionResult<List<DiskInfo>> GetDiskUsage()
        {
            var diskInfo = _performanceService.GetDiskInfo();
            return Ok(diskInfo);
        }

        /// <summary>
        /// Process bazlı detaylı bilgiler
        /// </summary>
        [HttpGet("process-details")]
        public ActionResult<ProcessDetails> GetProcessDetails()
        {
            var processDetails = _performanceService.GetProcessDetails();
            return Ok(processDetails);
        }
    }

    // Service Interface
    public interface IPerformanceService
    {
        PerformanceMetrics GetCurrentMetrics();
        SystemInfo GetSystemInfo();
        Task<List<PerformanceSnapshot>> MonitorAsync(int durationSeconds, int intervalSeconds);
        List<DiskInfo> GetDiskInfo();
        ProcessDetails GetProcessDetails();
    }

    // Service Implementation
    public class PerformanceService : IPerformanceService
    {
        private readonly Process _currentProcess;
        private DateTime _lastCpuCheck;
        private TimeSpan _lastTotalProcessorTime;

        public PerformanceService()
        {
            _currentProcess = Process.GetCurrentProcess();
            _lastCpuCheck = DateTime.UtcNow;
            _lastTotalProcessorTime = _currentProcess.TotalProcessorTime;
        }

        public PerformanceMetrics GetCurrentMetrics()
        {
            _currentProcess.Refresh();

            var cpuUsage = GetCpuUsage();
            var memoryUsageMB = _currentProcess.WorkingSet64 / 1024.0 / 1024.0;
            var privateMemoryMB = _currentProcess.PrivateMemorySize64 / 1024.0 / 1024.0;

            var gcInfo = new GCInfo
            {
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                TotalMemoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0
            };

            return new PerformanceMetrics
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = cpuUsage,
                MemoryUsageMB = memoryUsageMB,
                PrivateMemoryMB = privateMemoryMB,
                ThreadCount = _currentProcess.Threads.Count,
                HandleCount = _currentProcess.HandleCount,
                GCInfo = gcInfo,
                UptimeSeconds = (DateTime.UtcNow - _currentProcess.StartTime).TotalSeconds
            };
        }

        public SystemInfo GetSystemInfo()
        {
            return new SystemInfo
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                SystemPageSize = Environment.SystemPageSize,
                WorkingSet = Environment.WorkingSet / 1024.0 / 1024.0,
                RuntimeVersion = Environment.Version.ToString(),
                ProcessId = _currentProcess.Id,
                ProcessName = _currentProcess.ProcessName,
                ProcessStartTime = _currentProcess.StartTime
            };
        }

        public async Task<List<PerformanceSnapshot>> MonitorAsync(int durationSeconds, int intervalSeconds)
        {
            var snapshots = new List<PerformanceSnapshot>();
            var iterations = durationSeconds / intervalSeconds;

            for (int i = 0; i < iterations; i++)
            {
                var metrics = GetCurrentMetrics();
                snapshots.Add(new PerformanceSnapshot
                {
                    SequenceNumber = i + 1,
                    Metrics = metrics
                });

                if (i < iterations - 1)
                    await Task.Delay(intervalSeconds * 1000);
            }

            return snapshots;
        }

        public List<DiskInfo> GetDiskInfo()
        {
            var diskInfoList = new List<DiskInfo>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    diskInfoList.Add(new DiskInfo
                    {
                        Name = drive.Name,
                        DriveType = drive.DriveType.ToString(),
                        TotalSizeGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0,
                        AvailableFreeSpaceGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0,
                        UsedSpaceGB = (drive.TotalSize - drive.AvailableFreeSpace) / 1024.0 / 1024.0 / 1024.0,
                        UsagePercent = ((drive.TotalSize - drive.AvailableFreeSpace) * 100.0) / drive.TotalSize,
                        VolumeLabel = drive.VolumeLabel,
                        DriveFormat = drive.DriveFormat
                    });
                }
            }

            return diskInfoList;
        }

        public ProcessDetails GetProcessDetails()
        {
            _currentProcess.Refresh();

            return new ProcessDetails
            {
                ProcessId = _currentProcess.Id,
                ProcessName = _currentProcess.ProcessName,
                StartTime = _currentProcess.StartTime,
                TotalProcessorTimeSeconds = _currentProcess.TotalProcessorTime.TotalSeconds,
                UserProcessorTimeSeconds = _currentProcess.UserProcessorTime.TotalSeconds,
                PrivilegedProcessorTimeSeconds = _currentProcess.PrivilegedProcessorTime.TotalSeconds,
                VirtualMemoryMB = _currentProcess.VirtualMemorySize64 / 1024.0 / 1024.0,
                PeakVirtualMemoryMB = _currentProcess.PeakVirtualMemorySize64 / 1024.0 / 1024.0,
                PeakWorkingSetMB = _currentProcess.PeakWorkingSet64 / 1024.0 / 1024.0,
                PagedMemoryMB = _currentProcess.PagedMemorySize64 / 1024.0 / 1024.0,
                NonpagedSystemMemoryMB = _currentProcess.NonpagedSystemMemorySize64 / 1024.0 / 1024.0
            };
        }

        private double GetCpuUsage()
        {
            var currentTime = DateTime.UtcNow;
            var currentTotalProcessorTime = _currentProcess.TotalProcessorTime;

            var timeDiff = (currentTime - _lastCpuCheck).TotalMilliseconds;
            var processorTimeDiff = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds;

            var cpuUsage = (processorTimeDiff / (Environment.ProcessorCount * timeDiff)) * 100;

            _lastCpuCheck = currentTime;
            _lastTotalProcessorTime = currentTotalProcessorTime;

            return Math.Round(cpuUsage, 2);
        }
    }

    // DTOs
    public class PerformanceMetrics
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsagePercent { get; set; }
        public double MemoryUsageMB { get; set; }
        public double PrivateMemoryMB { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public GCInfo GCInfo { get; set; }
        public double UptimeSeconds { get; set; }
    }

    public class GCInfo
    {
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public double TotalMemoryMB { get; set; }
    }

    public class SystemInfo
    {
        public string MachineName { get; set; }
        public string OSVersion { get; set; }
        public int ProcessorCount { get; set; }
        public bool Is64BitOperatingSystem { get; set; }
        public bool Is64BitProcess { get; set; }
        public int SystemPageSize { get; set; }
        public double WorkingSet { get; set; }
        public string RuntimeVersion { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public DateTime ProcessStartTime { get; set; }
    }

    public class PerformanceSnapshot
    {
        public int SequenceNumber { get; set; }
        public PerformanceMetrics Metrics { get; set; }
    }

    public class DiskInfo
    {
        public string Name { get; set; }
        public string DriveType { get; set; }
        public double TotalSizeGB { get; set; }
        public double AvailableFreeSpaceGB { get; set; }
        public double UsedSpaceGB { get; set; }
        public double UsagePercent { get; set; }
        public string VolumeLabel { get; set; }
        public string DriveFormat { get; set; }
    }

    public class ProcessDetails
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public DateTime StartTime { get; set; }
        public double TotalProcessorTimeSeconds { get; set; }
        public double UserProcessorTimeSeconds { get; set; }
        public double PrivilegedProcessorTimeSeconds { get; set; }
        public double VirtualMemoryMB { get; set; }
        public double PeakVirtualMemoryMB { get; set; }
        public double PeakWorkingSetMB { get; set; }
        public double PagedMemoryMB { get; set; }
        public double NonpagedSystemMemoryMB { get; set; }
    }
}