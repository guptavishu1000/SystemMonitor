using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Providers;

public sealed class LinuxMetricsProvider : ISystemMetricsProvider
{
    private CpuSample _lastSample;

    public LinuxMetricsProvider()
    {
//#if LINUX
//        _lastSample = ReadCpuSample();
//#else
        throw new PlatformNotSupportedException("LinuxMetricsProvider can only be used on Linux systems");
//#endif
    }

    public MonitoringData GetMetrics()
    {
        // CPU: read /proc/stat and compute percent since last call
        var current = ReadCpuSample();
        var cpuPercent = CalculateCpuUsagePercent(_lastSample, current);
        _lastSample = current;

        // Memory: read /proc/meminfo
        var (totalMb, usedMb) = ReadMemInfo();

        // Disk
        var root = Path.GetPathRoot(AppContext.BaseDirectory) ?? "/";
        var drive = new DriveInfo(root);
        double diskTotal = drive.TotalSize / (1024.0 * 1024.0);
        double diskFree = drive.AvailableFreeSpace / (1024.0 * 1024.0);
        double diskUsed = Math.Max(0, diskTotal - diskFree);

        return new MonitoringData
        {
            CpuPercent = Math.Round(cpuPercent, 1),
            RamUsedMb = Math.Round(usedMb, 1),
            RamTotalMb = Math.Round(totalMb, 1),
            DiskUsedMb = Math.Round(diskUsed, 1),
            DiskTotalMb = Math.Round(diskTotal, 1),
        };
    }

    private static CpuSample ReadCpuSample()
    {
        var text = File.ReadAllLines("/proc/stat");
        var line = text.FirstOrDefault(l => l.StartsWith("cpu ")) ?? text[0];
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // parts: ["cpu", user, nice, system, idle, iowait, irq, softirq, steal, ...]
        if (parts.Length < 5) throw new InvalidOperationException("Unexpected /proc/stat format");
        ulong user = ulong.Parse(parts[1]);
        ulong nice = ulong.Parse(parts[2]);
        ulong system = ulong.Parse(parts[3]);
        ulong idle = ulong.Parse(parts[4]);
        ulong iowait = parts.Length > 5 ? ulong.Parse(parts[5]) : 0;
        ulong irq = parts.Length > 6 ? ulong.Parse(parts[6]) : 0;
        ulong softirq = parts.Length > 7 ? ulong.Parse(parts[7]) : 0;
        ulong steal = parts.Length > 8 ? ulong.Parse(parts[8]) : 0;
        ulong total = user + nice + system + idle + iowait + irq + softirq + steal;
        return new CpuSample(total, idle);
    }

    private static double CalculateCpuUsagePercent(CpuSample prev, CpuSample cur)
    {
        var totalDiff = (double)(cur.Total - prev.Total);
        if (totalDiff <= 0) return 0;
        var idleDiff = (double)(cur.Idle - prev.Idle);
        var usage = (1.0 - (idleDiff / totalDiff)) * 100.0;
        return Math.Clamp(usage, 0, 100);
    }

    private static (double totalMb, double usedMb) ReadMemInfo()
    {
        var lines = File.ReadAllLines("/proc/meminfo");
        // MemTotal:     16384256 kB
        // MemAvailable: 12345678 kB
        double totalKb = 0, availKb = 0;
        foreach (var l in lines)
        {
            if (l.StartsWith("MemTotal:")) totalKb = ExtractKb(l);
            else if (l.StartsWith("MemAvailable:")) availKb = ExtractKb(l);
            if (totalKb > 0 && availKb > 0) break;
        }
        var totalMb = totalKb / 1024.0;
        var usedMb = Math.Max(0, (totalKb - availKb) / 1024.0);
        return (totalMb, usedMb);

        static double ExtractKb(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return 0;
            return double.Parse(parts[1]);
        }
    }

    private record CpuSample(ulong Total, ulong Idle);
}