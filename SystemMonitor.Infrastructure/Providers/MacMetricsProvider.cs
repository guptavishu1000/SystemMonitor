using System.Diagnostics;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Providers;

public sealed class MacMetricsProvider : ISystemMetricsProvider
{
    private (ulong Total, ulong Idle) _lastCpu = (0, 0);

    public MacMetricsProvider()
    {
        _lastCpu = ReadCpu();
    }

    public MonitoringData GetMetrics()
    {
        var (total, idle) = ReadCpu();
        double cpu = 0;
        var totalDiff = (double)(total - _lastCpu.Total);
        if (totalDiff > 0)
        {
            var idleDiff = (double)(idle - _lastCpu.Idle);
            cpu = (1.0 - (idleDiff / totalDiff)) * 100.0;
        }
        _lastCpu = (total, idle);

        // Memory: use sysctl hw.memsize for total, and vm_stat to compute free
        var totalBytes = GetSysctlHwMemsize();
        var (usedMb, totalMb) = ReadMacMemory(totalBytes);

        // Disk
        var root = Path.GetPathRoot(AppContext.BaseDirectory) ?? "/";
        var drive = new DriveInfo(root);
        double diskTotal = drive.TotalSize / (1024.0 * 1024.0);
        double diskFree = drive.AvailableFreeSpace / (1024.0 * 1024.0);
        double diskUsed = Math.Max(0, diskTotal - diskFree);

        return new MonitoringData
        {
            CpuPercent = Math.Round(cpu, 1),
            RamUsedMb = Math.Round(usedMb, 1),
            RamTotalMb = Math.Round(totalMb, 1),
            DiskUsedMb = Math.Round(diskUsed, 1),
            DiskTotalMb = Math.Round(diskTotal, 1),
        };
    }

    private static (ulong total, ulong idle) ReadCpu()
    {
        // Use 'sar -u' isn't available by default. Instead use 'top -l 2 -n 0' and parse CPU line.
        // To keep dependencies minimal, sample using 'top' and parse the overall CPU usage lines.

        var psi = new ProcessStartInfo("/usr/bin/top", "-l 2 -n 0") { RedirectStandardOutput = true };
        using var p = Process.Start(psi);
        var outp = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        // The last repetition contains a line like: "CPU usage: 3.29% user, 5.43% sys, 91.26% idle"
        var cpuLines = outp.Split('\n').Where(l => l.Contains("CPU usage:"));
        var last = cpuLines.LastOrDefault();
        if (last == null) return (0, 0);
        // Parse numbers
        // Extract percentages
        var parts = last.Split(',');
        double user = 0, sys = 0, idle = 0;
        foreach (var ppart in parts)
        {
            var t = ppart.Trim();
            if (t.EndsWith("idle") && TryParseLeadingDouble(t, out var v)) idle = v;
            else if (t.EndsWith("sys") && TryParseLeadingDouble(t, out var v2)) sys = v2;
            else if (t.Contains("user") && TryParseLeadingDouble(t, out var v3)) user = v3;
        }
        // Build synthetic total/idle values scaled to 100 (not absolute counters). We'll return 100 as total and idle as percentage.
        // The CalculateCpu method expects absolute counters; to keep compatibility, we'll return (10000, idle*100)
        var total = (ulong)10000;
        var idleAbs = (ulong)(idle * 100);
        return (total, idleAbs);

        static bool TryParseLeadingDouble(string text, out double val)
        {
            val = 0;
            var i = 0; while (i < text.Length && (char.IsDigit(text[i]) || text[i] == '.' || text[i] == ' ')) i++;
            var candidate = new string(text.Take(i).ToArray()).Trim();
            return double.TryParse(candidate.Trim().TrimEnd('%'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out val);
        }
    }

    private static ulong GetSysctlHwMemsize()
    {
        var psi = new ProcessStartInfo("/usr/sbin/sysctl", "-n hw.memsize") { RedirectStandardOutput = true };
        using var p = Process.Start(psi);
        var outp = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        if (ulong.TryParse(outp, out var val)) return val;
        return 0;
    }

    private static (double usedMb, double totalMb) ReadMacMemory(ulong totalBytes)
    {
        // vm_stat output contains page counts. Each page is typically 4096 bytes.
        var psi = new ProcessStartInfo("/usr/bin/vm_stat", "") { RedirectStandardOutput = true };
        using var p = Process.Start(psi);
        var outp = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        var lines = outp.Split('\n');
        ulong freePages = 0, inactive = 0, speculative = 0;
        foreach (var l in lines)
        {
            if (l.StartsWith("Pages free:")) freePages = ExtractPages(l);
            else if (l.StartsWith("Pages inactive:")) inactive = ExtractPages(l);
            else if (l.StartsWith("Pages speculative:")) speculative = ExtractPages(l);
        }
        ulong pageSize = 4096;
        ulong freeBytes = (freePages + inactive + speculative) * pageSize;
        double totalMb = totalBytes / (1024.0 * 1024.0);
        double usedMb = Math.Max(0, (double)(totalBytes - freeBytes) / (1024.0 * 1024.0));
        return (usedMb, totalMb);

        static ulong ExtractPages(string line)
        {
            var parts = line.Split(':');
            if (parts.Length < 2) return 0;
            var num = new string(parts[1].Where(c => char.IsDigit(c)).ToArray());
            if (ulong.TryParse(num, out var v)) return v; return 0;
        }
    }
}