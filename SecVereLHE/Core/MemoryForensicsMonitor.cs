using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SecVerseLHE.Core
{
    internal sealed class MemoryForensicsMonitor : IDisposable
    {
        private const int ProcessQueryLimitedInformation = 0x1000;
        private const int ProcessVmRead = 0x0010;
        private const int ThreadQueryInformation = 0x0040;
        private const int ThreadGetContext = 0x0008;
        private const int ThreadSuspendResume = 0x0002;

        private const uint MemCommit = 0x1000;
        private const uint MemPrivate = 0x20000;

        private const uint PageExecute = 0x10;
        private const uint PageExecuteRead = 0x20;
        private const uint PageExecuteReadWrite = 0x40;
        private const uint PageExecuteWriteCopy = 0x80;

        private readonly ManualResetEventSlim _stopSignal = new ManualResetEventSlim(false);
        private readonly int _intervalMs;
        private readonly int _maxProcessesPerCycle;
        private readonly int _maxRegionsPerProcess;
        private readonly int _maxThreadsPerProcess;
        private readonly bool _enableContextCheck;
        private readonly bool _skipContextCheckForCritical;
        private Thread _worker;

        public event Action<MemoryFinding> SuspiciousRegionFound;
        public event Action<ThreadFinding> SuspiciousThreadFound;

        public MemoryForensicsMonitor(int intervalMs = 3000, int maxProcessesPerCycle = 8, int maxRegionsPerProcess = 64, int maxThreadsPerProcess = 64, bool enableContextCheck = false, bool skipContextCheckForCritical = true)
        {
            _intervalMs = Clamp(intervalMs, 1000, 60000);
            _maxProcessesPerCycle = Clamp(maxProcessesPerCycle, 1, 64);
            _maxRegionsPerProcess = Clamp(maxRegionsPerProcess, 1, 512);
            _maxThreadsPerProcess = Clamp(maxThreadsPerProcess, 1, 1024);
            _enableContextCheck = enableContextCheck;
            _skipContextCheckForCritical = skipContextCheckForCritical;
        }

        public void Start()
        {
            if (_worker != null) return;

            _stopSignal.Reset();
            _worker = new Thread(ScanLoop)
            {
                IsBackground = true,
                Name = "LHE.MemoryForensics",
                Priority = ThreadPriority.Lowest
            };
            _worker.Start();
        }

        public void Stop()
        {
            if (_worker == null) return;

            _stopSignal.Set();
            if (!_worker.Join(2000))
            {
                _worker.Interrupt();
            }
            _worker = null;
        }

        public void Dispose()
        {
            Stop();
            _stopSignal.Dispose();
        }

        private void ScanLoop()
        {
            var stopwatch = new Stopwatch();

            while (!_stopSignal.IsSet)
            {
                stopwatch.Restart();
                try
                {
                    ScanCycle();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHE: MemoryForensicsMonitor error: {ex.Message}");
                }

                stopwatch.Stop();
                var elapsed = (int)stopwatch.ElapsedMilliseconds;
                var waitMs = elapsed > _intervalMs ? Math.Min(elapsed + 1000, _intervalMs * 2) : _intervalMs;

                if (_stopSignal.Wait(waitMs))
                {
                    break;
                }
            }
        }

        private void ScanCycle()
        {
            var processes = Process.GetProcesses();
            var scanned = 0;
            var currentPid = Process.GetCurrentProcess().Id;

            foreach (var process in processes)
            {
                using (process)
                {
                    if (scanned >= _maxProcessesPerCycle) break;

                    var pid = process.Id;
                    if (pid == currentPid) continue;

                    try
                    {
                        if (ScanProcess(process, pid))
                        {
                            scanned++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LHE: ScanProcess error: {ex.Message}");
                    }
                }
            }
        }

        private bool ScanProcess(Process process, int pid)
        {
            var handle = OpenProcess(ProcessQueryLimitedInformation | ProcessVmRead, false, pid);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            List<MemoryRegion> suspiciousRegions = null;
            try
            {
                var regionsScanned = 0;
                long address = 0;
                var info = new MEMORY_BASIC_INFORMATION();
                var infoSize = (IntPtr)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));

                while (regionsScanned < _maxRegionsPerProcess)
                {
                    var result = VirtualQueryEx(handle, (IntPtr)address, out info, infoSize);
                    if (result == IntPtr.Zero)
                    {
                        break;
                    }

                    if (IsSuspicious(info))
                    {
                        if (suspiciousRegions == null)
                        {
                            suspiciousRegions = new List<MemoryRegion>(4);
                        }

                        suspiciousRegions.Add(new MemoryRegion(info.BaseAddress, info.RegionSize));
                        var handler = SuspiciousRegionFound;
                        if (handler != null)
                        {
                            handler(new MemoryFinding(pid,
                                SafeProcessName(process),
                                info.BaseAddress,
                                info.RegionSize,
                                info.Protect));
                        }
                    }

                    var next = info.BaseAddress.ToInt64() + (long)info.RegionSize;
                    if (next <= address) break;

                    address = next;
                    regionsScanned++;
                }

                if (suspiciousRegions != null && suspiciousRegions.Count > 0)
                {
                    ScanThreads(process, pid, suspiciousRegions);
                }
            }
            finally
            {
                CloseHandle(handle);
            }

            return true;
        }

        private static bool IsSuspicious(MEMORY_BASIC_INFORMATION info)
        {
            if (info.State != MemCommit) return false;
            if (info.Type != MemPrivate) return false;

            var executable = (info.Protect & (PageExecute | PageExecuteRead | PageExecuteReadWrite | PageExecuteWriteCopy)) != 0;
            var writable = (info.Protect & (PageExecuteReadWrite | PageExecuteWriteCopy)) != 0;

            return executable && writable;
        }

        private void ScanThreads(Process process, int pid, List<MemoryRegion> suspiciousRegions)
        {
            var scannedThreads = 0;
            var processName = SafeProcessName(process);
            var allowContextCheck = _enableContextCheck && (!_skipContextCheckForCritical || !IsCriticalProcess(processName));

            foreach (ProcessThread thread in process.Threads)
            {
                if (scannedThreads >= _maxThreadsPerProcess) break;

                var threadId = thread.Id;
                IntPtr threadHandle = IntPtr.Zero;
                try
                {
                    threadHandle = OpenThread(ThreadQueryInformation | ThreadGetContext | ThreadSuspendResume, false, threadId);
                    if (threadHandle == IntPtr.Zero)
                    {
                        threadHandle = OpenThread(ThreadQueryInformation | ThreadGetContext | ThreadSuspendResume, false, threadId);
                    }

                    if (threadHandle == IntPtr.Zero) continue;

                    if (TryGetThreadStartAddress(threadHandle, out var startAddress))
                    {
                        var startInRegion = IsAddressInRegions(startAddress, suspiciousRegions);
                        var instructionPointer = IntPtr.Zero;
                        var instructionInRegion = false;

                        if (allowContextCheck)
                        {
                            instructionInRegion = TryGetInstructionPointer(threadHandle, suspiciousRegions, out instructionPointer);
                        }

                        if (startInRegion || instructionInRegion)
                        {
                            var handler = SuspiciousThreadFound;
                            if (handler != null)
                            {
                                handler(new ThreadFinding(pid,
                                    processName,
                                    threadId,
                                    startAddress,
                                    instructionPointer,
                                    startInRegion,
                                    instructionInRegion));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHE: ScanThreads error: {ex.Message}");
                }
                finally
                {
                    if (threadHandle != IntPtr.Zero)
                    {
                        CloseHandle(threadHandle);
                    }
                }

                scannedThreads++;
            }
        }

        private static bool TryGetThreadStartAddress(IntPtr threadHandle, out IntPtr startAddress)
        {
            startAddress = IntPtr.Zero;
            var status = NtQueryInformationThread(threadHandle, 9, out startAddress, IntPtr.Size, IntPtr.Zero);
            return status == 0 && startAddress != IntPtr.Zero;
        }

        private static bool TryGetInstructionPointer(IntPtr threadHandle, List<MemoryRegion> suspiciousRegions, out IntPtr instructionPointer)
        {
            instructionPointer = IntPtr.Zero;
            if (!SuspendThreadSafe(threadHandle)) return false;

            try
            {
                if (IntPtr.Size == 8)
                {
                    var context = new CONTEXT64 { ContextFlags = ContextControl64 };
                    if (!GetThreadContext(threadHandle, ref context)) return false;
                    instructionPointer = new IntPtr((long)context.Rip);
                }
                else
                {
                    var context = new CONTEXT32 { ContextFlags = ContextControl32 };
                    if (!GetThreadContext32(threadHandle, ref context)) return false;
                    instructionPointer = new IntPtr(context.Eip);
                }

                return instructionPointer != IntPtr.Zero && IsAddressInRegions(instructionPointer, suspiciousRegions);
            }
            finally
            {
                ResumeThreadSafe(threadHandle);
            }
        }

        private static bool SuspendThreadSafe(IntPtr threadHandle)
        {
            return SuspendThread(threadHandle) != uint.MaxValue;
        }

        private static void ResumeThreadSafe(IntPtr threadHandle)
        {
            ResumeThread(threadHandle);
        }

        private static bool IsAddressInRegions(IntPtr address, List<MemoryRegion> regions)
        {
            var value = address.ToInt64();
            for (var i = 0; i < regions.Count; i++)
            {
                if (regions[i].Contains(value)) return true;
            }

            return false;
        }

        private static string SafeProcessName(Process process)
        {
            try
            {
                return process.ProcessName;
            }
            catch
            {
                return "unknown";
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static bool IsCriticalProcess(string processName)
        {
            return CriticalProcessNames.Contains(processName);
        }

        private static readonly HashSet<string> CriticalProcessNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "csrss",
            "wininit",
            "winlogon",
            "services",
            "lsass",
            "smss"
        };

        private readonly struct MemoryRegion
        {
            private readonly long _start;
            private readonly long _end;

            public MemoryRegion(IntPtr baseAddress, UIntPtr size)
            {
                _start = baseAddress.ToInt64();
                var regionSize = (long)size;
                _end = regionSize <= 0 ? _start : _start + regionSize;
            }

            public bool Contains(long address)
            {
                return address >= _start && address < _end;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        internal readonly struct MemoryFinding
        {
            public MemoryFinding(int processId, string processName, IntPtr baseAddress, UIntPtr regionSize, uint protection)
            {
                ProcessId = processId;
                ProcessName = processName;
                BaseAddress = baseAddress;
                RegionSize = regionSize;
                Protection = protection;
            }

            public int ProcessId { get; }
            public string ProcessName { get; }
            public IntPtr BaseAddress { get; }
            public UIntPtr RegionSize { get; }
            public uint Protection { get; }
        }

        internal readonly struct ThreadFinding
        {
            public ThreadFinding(int processId, string processName, int threadId, IntPtr startAddress, IntPtr instructionPointer, bool startInRegion, bool instructionInRegion)
            {
                ProcessId = processId;
                ProcessName = processName;
                ThreadId = threadId;
                StartAddress = startAddress;
                InstructionPointer = instructionPointer;
                StartInRegion = startInRegion;
                InstructionInRegion = instructionInRegion;
            }

            public int ProcessId { get; }
            public string ProcessName { get; }
            public int ThreadId { get; }
            public IntPtr StartAddress { get; }
            public IntPtr InstructionPointer { get; }
            public bool StartInRegion { get; }
            public bool InstructionInRegion { get; }
        }

        private const uint ContextControl64 = 0x00100001;
        private const uint ContextControl32 = 0x00010001;

        [StructLayout(LayoutKind.Sequential)]
        private struct CONTEXT64
        {
            public ulong P1Home;
            public ulong P2Home;
            public ulong P3Home;
            public ulong P4Home;
            public ulong P5Home;
            public ulong P6Home;
            public uint ContextFlags;
            public uint MxCsr;
            public ushort SegCs;
            public ushort SegDs;
            public ushort SegEs;
            public ushort SegFs;
            public ushort SegGs;
            public ushort SegSs;
            public uint EFlags;
            public ulong Dr0;
            public ulong Dr1;
            public ulong Dr2;
            public ulong Dr3;
            public ulong Dr6;
            public ulong Dr7;
            public ulong Rax;
            public ulong Rcx;
            public ulong Rdx;
            public ulong Rbx;
            public ulong Rsp;
            public ulong Rbp;
            public ulong Rsi;
            public ulong Rdi;
            public ulong R8;
            public ulong R9;
            public ulong R10;
            public ulong R11;
            public ulong R12;
            public ulong R13;
            public ulong R14;
            public ulong R15;
            public ulong Rip;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONTEXT32
        {
            public uint ContextFlags;
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            public uint Ebp;
            public uint Eip;
            public uint SegCs;
            public uint EFlags;
            public uint Esp;
            public uint SegSs;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int access, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenThread(int access, bool inheritHandle, int threadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, IntPtr dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        [DllImport("kernel32.dll", EntryPoint = "GetThreadContext", SetLastError = true)]
        private static extern bool GetThreadContext32(IntPtr hThread, ref CONTEXT32 lpContext);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationThread(IntPtr threadHandle, int threadInformationClass, out IntPtr threadInformation, int threadInformationLength, IntPtr returnLengthPtr);
    }
}
