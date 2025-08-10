using System.Diagnostics;
using System.Runtime.InteropServices;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Providers;

public sealed class WindowsMetricsProvider : ISystemMetricsProvider
{
    private readonly PerformanceCounter _cpuCounter;

    public WindowsMetricsProvider()
    {
        // Use PerformanceCounter for Windows CPU total
//#if WINDOWS
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _cpuCounter.NextValue(); // prime
//#else
//        throw new PlatformNotSupportedException("WindowsMetricsProvider is only supported on Windows.");
//#endif
    }

    public MonitoringData GetMetrics()
    {
        // CPU
        // PerformanceCounter needs a short delay between samples for accurate value;
        // however calling NextValue twice with a delay is expensive inside GetMetrics; instead
        // we sleep 500ms to allow a more correct reading. This design keeps GetMetrics blocking briefly.
        Thread.Sleep(500);
        float cpu = _cpuCounter.NextValue();

        // Memory
        var mem = new MEMORYSTATUSEX();
        if (!GlobalMemoryStatusEx(mem))
            throw new InvalidOperationException("Failed to query memory status");
        double totalMb = mem.ullTotalPhys / (1024.0 * 1024.0);
        double availMb = mem.ullAvailPhys / (1024.0 * 1024.0);
        double usedMb = Math.Max(0, totalMb - availMb);

        // Disk (current drive)
        var root = Path.GetPathRoot(AppContext.BaseDirectory) ?? "C:\\";
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

    #region Native Memory Query
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    #endregion
}