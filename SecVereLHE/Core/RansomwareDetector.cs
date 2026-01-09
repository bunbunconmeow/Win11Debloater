using SecVerseLHE.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SecVerseLHE.Helper.ThreadManager;

namespace SecVerseLHE.Core
{
    internal class RansomwareDetector : IManagedThreadWorker, IDisposable
    {

        #region Configuration
        private const int IMMEDIATE_BLOCK_THRESHOLD = 20;
        private const int OBSERVATION_THRESHOLD = 8;
        private const int OBSERVATION_WINDOW_MS = 3000;
        private const int SUSTAINED_BLOCK_THRESHOLD = 50;
        private const double HIGH_ENTROPY_THRESHOLD = 7;     
        private const int ENTROPY_SAMPLE_SIZE = 4096;          
        private static readonly HashSet<string> SafeProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "explorer", "searchindexer", "msiexec", "trustedinstaller",
            "tiworker", "windows.immersiveshell", "runtimebroker",
            "devenv", "code", "rider64", "msbuild", "dotnet",
            "7z", "7zfm", "winrar", "winzip", "peazip",
            "onedrive", "dropbox", "googledrivesync",
            "steam", "epicgameslauncher", "origin"
        };

        private static readonly HashSet<string> MonitoredExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".pdf",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".psd", ".ai",
            ".mp3", ".mp4", ".avi", ".mkv", ".mov",
            ".zip", ".rar", ".7z", ".tar", ".gz",
            ".sql", ".mdb", ".accdb", ".sqlite",
            ".cs", ".java", ".py", ".cpp", ".h", ".js", ".ts",
            ".txt", ".rtf", ".csv", ".xml", ".json", ".yaml"
        };

        #endregion Configuration

        #region Fields

        private readonly TrayMessageDispatcher _dispatcher;
        private readonly TrayManager _trayManager;
        private CancellationToken _cancellationToken;

        // Process tracking PID into Activity data
        private readonly ConcurrentDictionary<int, ProcessActivity> _processActivity;

        // Suspended processes awaiting user decision
        private readonly ConcurrentDictionary<int, SuspendedProcessInfo> _suspendedProcesses;

        // File system watchers
        private readonly List<FileSystemWatcher> _watchers;
        private readonly object _watcherLock = new object();

        private volatile bool _disposed;
        private volatile bool _isRunning;

        #endregion Fields

        #region Nested Types

        private class ProcessActivity
        {
            public string ProcessName { get; set; }
            public string ProcessPath { get; set; }
            public ConcurrentQueue<FileModification> Modifications { get; } = new ConcurrentQueue<FileModification>();
            public int TotalModifications;
            public bool IsUnderObservation;
            public DateTime ObservationStarted;
            public bool IsBlocked;
        }

        private readonly struct FileModification
        {
            public DateTime Timestamp { get; }
            public string FilePath { get; }
            public double? Entropy { get; }
            public WatcherChangeTypes ChangeType { get; }

            public FileModification(string path, WatcherChangeTypes type, double? entropy)
            {
                Timestamp = DateTime.UtcNow;
                FilePath = path;
                ChangeType = type;
                Entropy = entropy;
            }
        }

        private class SuspendedProcessInfo
        {
            public int ProcessId { get; set; }
            public string ProcessName { get; set; }
            public string ProcessPath { get; set; }
            public DateTime SuspendedAt { get; set; }
            public int ModificationCount { get; set; }
            public List<string> AffectedFiles { get; set; }
        }

        #endregion Nested Types
        public RansomwareDetector(TrayManager trayManager, TrayMessageDispatcher dispatcher)
        {
            _trayManager = trayManager ?? throw new ArgumentNullException(nameof(trayManager));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _processActivity = new ConcurrentDictionary<int, ProcessActivity>();
            _suspendedProcesses = new ConcurrentDictionary<int, SuspendedProcessInfo>();
            _watchers = new List<FileSystemWatcher>();
        }

        #region IManagedThreadWorker Implementation

        public void Initialize(CancellationToken token)
        {
            _cancellationToken = token;
            InitializeWatchers();
        }

        public void Execute()
        {
            _isRunning = true;

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Periodic cleanup of old activity data
                    CleanupOldActivity();

                    // Check observed processes
                    CheckObservedProcesses();

                    Thread.Sleep(1000);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHE RansomwareDetector: Error in main loop: {ex.Message}");
                }
            }

            _isRunning = false;
        }

        public void Cleanup()
        {
            _isRunning = false;
            DisposeWatchers();

            // Resume any suspended processes on shutdown
            foreach (var kvp in _suspendedProcesses)
            {
                try
                {
                    ResumeProcess(kvp.Key);
                }
                catch { }
            }
        }

        public void OnError(Exception ex)
        {
            _dispatcher.Enqueue("Ransomware Detection Error",
                $"Detection service encountered an error: {ex.Message}");
        }

        #endregion

        #region File System Monitoring

        private void InitializeWatchers()
        {
            lock (_watcherLock)
            {
                // Monitor user profile folders
                var pathsToMonitor = new List<string>
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                };

                // Add additional drives
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Fixed && drive.IsReady)
                    {
                        pathsToMonitor.Add(drive.RootDirectory.FullName);
                    }
                }

                foreach (var path in pathsToMonitor.Distinct())
                {
                    if (!Directory.Exists(path)) continue;

                    try
                    {
                        var watcher = new FileSystemWatcher(path)
                        {
                            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                            IncludeSubdirectories = true,
                            EnableRaisingEvents = true,
                            InternalBufferSize = 65536 // 64kb
                        };

                        watcher.Changed += OnFileChanged;
                        watcher.Created += OnFileChanged;
                        watcher.Renamed += OnFileRenamed;
                        watcher.Error += OnWatcherError;

                        _watchers.Add(watcher);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LHE: Failed to create watcher for {path}: {ex.Message}");
                    }
                }
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_cancellationToken.IsCancellationRequested || !_isRunning)
                return;

            ProcessFileEvent(e.FullPath, e.ChangeType);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_cancellationToken.IsCancellationRequested || !_isRunning)
                return;

            // Renaming to suspicious extensions is a red flag
            ProcessFileEvent(e.FullPath, WatcherChangeTypes.Renamed);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"LHE: FileSystemWatcher error: {e.GetException().Message}");
        }

        private void ProcessFileEvent(string filePath, WatcherChangeTypes changeType)
        {
            try
            {
                // Skip if not a monitored extension -> Maybe ransomware is targeting other file types, but this reduces noise
                var ext = Path.GetExtension(filePath);
                if (string.IsNullOrEmpty(ext) || !MonitoredExtensions.Contains(ext))
                    return;

                // Skip some paths from the system
                if (IsSystemPath(filePath))
                    return;

                // Find the process that locked/modified the file
                var lockingProcessId = GetLockingProcessId(filePath);
                if (lockingProcessId <= 0)
                    return;

                // Skip safe processes
                var processName = GetProcessName(lockingProcessId);
                if (string.IsNullOrEmpty(processName) || SafeProcesses.Contains(processName))
                    return;

                // Calculate entropy for changed files
                double? entropy = null;
                if (changeType == WatcherChangeTypes.Changed && File.Exists(filePath))
                {
                    entropy = CalculateFileEntropy(filePath);
                }

                // Record the activity
                RecordActivity(lockingProcessId, filePath, changeType, entropy);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: Error processing file event: {ex.Message}");
            }
        }

        #endregion File System Monitoring

        #region Activity Tracking and Detection

        private void RecordActivity(int processId, string filePath, WatcherChangeTypes changeType, double? entropy)
        {
            var activity = _processActivity.GetOrAdd(processId, pid => new ProcessActivity
            {
                ProcessName = GetProcessName(pid),
                ProcessPath = GetProcessPath(pid)
            });

            if (activity.IsBlocked)
                return;

            var modification = new FileModification(filePath, changeType, entropy);
            activity.Modifications.Enqueue(modification);
            Interlocked.Increment(ref activity.TotalModifications);

            // Get recent modifications
            var cutoff = DateTime.UtcNow.AddMilliseconds(-OBSERVATION_WINDOW_MS);
            var recentMods = activity.Modifications
                .Where(m => m.Timestamp > cutoff)
                .ToList();

            var recentCount = recentMods.Count;
            var highEntropyCount = recentMods.Count(m => m.Entropy.HasValue && m.Entropy.Value > HIGH_ENTROPY_THRESHOLD);

            // Check for immediate threat
            if (recentCount >= IMMEDIATE_BLOCK_THRESHOLD || highEntropyCount >= IMMEDIATE_BLOCK_THRESHOLD / 2)
            {
                HandleThreatDetected(processId, activity, recentMods, true);
                return;
            }

            // Start observation
            if (recentCount >= OBSERVATION_THRESHOLD && !activity.IsUnderObservation)
            {
                activity.IsUnderObservation = true;
                activity.ObservationStarted = DateTime.UtcNow;

                _dispatcher.Enqueue("Suspicious Activity Detected",
                    $"Process '{activity.ProcessName}' is modifying files rapidly. Monitoring...");
            }
        }

        private void CheckObservedProcesses()
        {
            foreach (var kvp in _processActivity)
            {
                var activity = kvp.Value;
                if (!activity.IsUnderObservation || activity.IsBlocked)
                    continue;

                // Check if sustained threshold exceeded
                if (activity.TotalModifications >= SUSTAINED_BLOCK_THRESHOLD)
                {
                    var recentMods = activity.Modifications.ToList();
                    HandleThreatDetected(kvp.Key, activity, recentMods, false);
                }
            }
        }

        private void HandleThreatDetected(int processId, ProcessActivity activity,
            List<FileModification> modifications, bool isImmediate)
        {
            if (activity.IsBlocked)
                return;

            activity.IsBlocked = true;

            try
            {
                // Suspend the process immediately
                SuspendProcess(processId);

                var suspendedInfo = new SuspendedProcessInfo
                {
                    ProcessId = processId,
                    ProcessName = activity.ProcessName,
                    ProcessPath = activity.ProcessPath,
                    SuspendedAt = DateTime.UtcNow,
                    ModificationCount = modifications.Count,
                    AffectedFiles = modifications.Select(m => m.FilePath).Distinct().Take(10).ToList()
                };

                _suspendedProcesses.TryAdd(processId, suspendedInfo);

                // Notify user
                _dispatcher.Enqueue("Ransomware Activity Blocked!",
                    $"Process '{activity.ProcessName}' (PID: {processId}) was suspended.\n" +
                    $"Modified {modifications.Count} files in {OBSERVATION_WINDOW_MS}ms.\n" +
                    "Check the popup to allow or terminate.");

                // Show decision dialog
                ShowDecisionDialog(suspendedInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: Failed to handle threat: {ex.Message}");
                _dispatcher.Enqueue("Ransomware Detection Error",
                    $"Failed to suspend suspicious process: {ex.Message}");
            }
        }

        private void ShowDecisionDialog(SuspendedProcessInfo info)
        {
            // Marshal to UI thread
            if (Application.OpenForms.Count > 0)
            {
                var form = Application.OpenForms[0];
                if (form.InvokeRequired)
                {
                    form.BeginInvoke(new Action(() => ShowDecisionDialogInternal(info)));
                    return;
                }
            }

            // Fallback: use ThreadPool to show dialog to usr
            ThreadPool.QueueUserWorkItem(_ => ShowDecisionDialogInternal(info));
        }

        private void ShowDecisionDialogInternal(SuspendedProcessInfo info)
        {
            var affectedList = string.Join("\n", info.AffectedFiles.Take(5));
            if (info.AffectedFiles.Count > 5)
                affectedList += $"\n... and {info.AffectedFiles.Count - 5} more";

            var message = $"POTENTIAL RANSOMWARE DETECTED!\n\n" +
                         $"Process: {info.ProcessName}\n" +
                         $"Path: {info.ProcessPath}\n" +
                         $"PID: {info.ProcessId}\n" +
                         $"Files Modified: {info.ModificationCount}\n\n" +
                         $"Affected Files:\n{affectedList}\n\n" +
                         $"Do you want to ALLOW this process to continue?\n" +
                         $"Click 'Yes' to resume, 'No' to terminate the process.";

            var result = MessageBox.Show(
                message,
                "SecVerse LHE - Ransomware Alert",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                // User trusts the process
                ResumeProcess(info.ProcessId);
                _suspendedProcesses.TryRemove(info.ProcessId, out _);

                // Add to temporary whitelist for this session
                if (!string.IsNullOrEmpty(info.ProcessName))
                    SafeProcesses.Add(info.ProcessName.ToLowerInvariant());

                _dispatcher.Enqueue("Process Resumed",
                    $"'{info.ProcessName}' has been allowed to continue.");
            }
            else
            {
                // Fuck over the the process
                TerminateProcess(info.ProcessId);
                _suspendedProcesses.TryRemove(info.ProcessId, out _);

                _dispatcher.Enqueue("Process Terminated",
                    $"'{info.ProcessName}' has been terminated as potential ransomware.");
            }
        }

        private void CleanupOldActivity()
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-30);

            foreach (var kvp in _processActivity.ToArray())
            {
                var activity = kvp.Value;

                // Remove old modifications from queue
                while (activity.Modifications.TryPeek(out var mod) && mod.Timestamp < cutoff)
                {
                    activity.Modifications.TryDequeue(out _);
                }

                // Remove inactive processes
                if (!activity.IsUnderObservation && !activity.IsBlocked &&
                    activity.Modifications.IsEmpty)
                {
                    _processActivity.TryRemove(kvp.Key, out _);
                }
            }
        }

        #endregion Activity Tracking and Detection

        #region Process Control

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSuspendProcess(IntPtr processHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtResumeProcess(IntPtr processHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr processHandle, uint exitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags,
            System.Text.StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames,
            uint nApplications, uint[] rgApplications, uint nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo,
            [In, Out] RM_PROCESS_INFO[] rgAffectedApps, ref uint lpdwRebootReasons);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RM_PROCESS_INFO
        {
            public uint Process_dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME Process_ProcessStartTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string strServiceShortName;
            public uint ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        private const uint PROCESS_SUSPEND_RESUME = 0x0800;
        private const uint PROCESS_TERMINATE = 0x0001;
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        private void SuspendProcess(int processId)
        {
            var handle = OpenProcess(PROCESS_SUSPEND_RESUME, false, processId);
            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Failed to open process {processId}");
            }

            try
            {
                var result = NtSuspendProcess(handle);
                if (result != 0)
                {
                    throw new InvalidOperationException($"NtSuspendProcess failed with code {result}");
                }
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private void ResumeProcess(int processId)
        {
            var handle = OpenProcess(PROCESS_SUSPEND_RESUME, false, processId);
            if (handle == IntPtr.Zero)
                return;

            try
            {
                NtResumeProcess(handle);
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private void TerminateProcess(int processId)
        {
            var handle = OpenProcess(PROCESS_TERMINATE, false, processId);
            if (handle == IntPtr.Zero)
                return;

            try
            {
                TerminateProcess(handle, 1);
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private string GetProcessName(int processId)
        {
            try
            {
                using (var process = Process.GetProcessById(processId))
                {
                    return process.ProcessName;
                }
            }
            catch
            {
                return null;
            }
        }

        private string GetProcessPath(int processId)
        {
            var handle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (handle == IntPtr.Zero)
                return null;

            try
            {
                var buffer = new System.Text.StringBuilder(1024);
                var size = buffer.Capacity;
                if (QueryFullProcessImageName(handle, 0, buffer, ref size))
                {
                    return buffer.ToString();
                }
            }
            finally
            {
                CloseHandle(handle);
            }

            return null;
        }

        private int GetLockingProcessId(string filePath)
        {
            try
            {
                if (RmStartSession(out uint sessionHandle, 0, Guid.NewGuid().ToString()) != 0)
                    return -1;

                try
                {
                    var files = new[] { filePath };
                    if (RmRegisterResources(sessionHandle, (uint)files.Length, files, 0, null, 0, null) != 0)
                        return -1;

                    uint pnProcInfoNeeded = 0;
                    uint pnProcInfo = 0;
                    uint lpdwRebootReasons = 0;

                    RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                    if (pnProcInfoNeeded == 0)
                        return -1;

                    var processes = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;

                    if (RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, processes, ref lpdwRebootReasons) == 0)
                    {
                        if (pnProcInfo > 0)
                        {
                            return (int)processes[0].Process_dwProcessId;
                        }
                    }
                }
                finally
                {
                    RmEndSession(sessionHandle);
                }
            }
            catch { }

            return -1;
        }

        #endregion Process Control

        #region Utility Methods

        private bool IsSystemPath(string path)
        {
            var lowerPath = path.ToLowerInvariant();
            return lowerPath.Contains("\\windows\\") ||
                   lowerPath.Contains("\\program files") ||
                   lowerPath.Contains("\\programdata\\") ||
                   lowerPath.Contains("\\appdata\\local\\temp\\") ||
                   lowerPath.Contains("\\$recycle.bin\\");
        }

        private double CalculateFileEntropy(string filePath)
        {
            try
            {
                var bytes = new byte[ENTROPY_SAMPLE_SIZE];
                int bytesRead;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bytesRead = fs.Read(bytes, 0, ENTROPY_SAMPLE_SIZE);
                }

                if (bytesRead == 0)
                    return 0;

                var frequency = new int[256];
                for (int i = 0; i < bytesRead; i++)
                {
                    frequency[bytes[i]]++;
                }

                double entropy = 0;
                for (int i = 0; i < 256; i++)
                {
                    if (frequency[i] > 0)
                    {
                        double p = (double)frequency[i] / bytesRead;
                        entropy -= p * Math.Log(p, 2);
                    }
                }

                return entropy;
            }
            catch
            {
                return 0;
            }
        }

        private void DisposeWatchers()
        {
            lock (_watcherLock)
            {
                foreach (var watcher in _watchers)
                {
                    try
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher.Changed -= OnFileChanged;
                        watcher.Created -= OnFileChanged;
                        watcher.Renamed -= OnFileRenamed;
                        watcher.Error -= OnWatcherError;
                        watcher.Dispose();
                    }
                    catch { }
                }
                _watchers.Clear();
            }
        }

        #endregion Utility Methods

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Cleanup();
        }
    }
}