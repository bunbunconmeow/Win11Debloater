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

        private const int IMMEDIATE_BLOCK_THRESHOLD = 60;
        private const int OBSERVATION_THRESHOLD = 8;
        private const int OBSERVATION_WINDOW_MS = 3000;
        private const int SUSTAINED_BLOCK_THRESHOLD = 60;
        private const double HIGH_ENTROPY_THRESHOLD = 7.5;
        private const int ENTROPY_SAMPLE_SIZE = 4096;


        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        private BlockingCollection<FileEventData> _eventQueue = new BlockingCollection<FileEventData>();
        private Task _processingTask;

        private static readonly HashSet<string> SafeProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "explorer", "searchindexer", "msiexec", "trustedinstaller",
            "tiworker", "windows.immersiveshell", "runtimebroker",
            "devenv", "code", "rider64", "msbuild", "dotnet",
            "7z", "7zfm", "winrar", "winzip", "peazip",
            "onedrive", "dropbox", "googledrivesync",
            "steam", "epicgameslauncher", "origin",
            "system", "idle", "csrss", "smss", "wininit",
            "svchost", "services", "lsass", "dwm", "taskhostw",
            "secverselhe", "secverse"
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

        private readonly ConcurrentDictionary<int, ProcessActivity> _processActivity;
        private readonly ConcurrentDictionary<int, SuspendedProcessInfo> _suspendedProcesses;
        private readonly HashSet<string> _sessionWhitelist;
        private readonly object _whitelistLock = new object();
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

        private struct FileModification
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

        #region Constructor

        public RansomwareDetector(TrayManager trayManager, TrayMessageDispatcher dispatcher)
        {
            _trayManager = trayManager ?? throw new ArgumentNullException(nameof(trayManager));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _processActivity = new ConcurrentDictionary<int, ProcessActivity>();
            _suspendedProcesses = new ConcurrentDictionary<int, SuspendedProcessInfo>();
            _sessionWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _watchers = new List<FileSystemWatcher>();
        }

        #endregion Constructor

        #region Path Validation

        private bool IsValidPath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return false;

                if (string.IsNullOrWhiteSpace(path))
                    return false;

                
                if (path.Length < 3 || path.Length > 32767)
                    return false;

                if (path.IndexOfAny(InvalidPathChars) >= 0)
                    return false;


                if (path.Length < 2 || path[1] != ':')
                {
                    if (!path.StartsWith(@"\\"))
                        return false;
                }

                string fileName = null;
                try
                {
                    int lastSep = path.LastIndexOfAny(new[] { '\\', '/' });
                    if (lastSep >= 0 && lastSep < path.Length - 1)
                    {
                        fileName = path.Substring(lastSep + 1);
                    }
                }
                catch
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    
                    foreach (char c in fileName)
                    {
                        if (c < 32) 
                            return false;

                        if (c == '<' || c == '>' || c == ':' || c == '"' ||
                            c == '|' || c == '?' || c == '*')
                            return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

    
        private string SanitizePath(string path)
        {
            try
            {
                if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return null;
                if (path.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return null;
                if (string.IsNullOrEmpty(path)) return null;
                path = path.Trim();
                if (string.IsNullOrEmpty(path)) return null;
                if (path.Contains('\0')) path = path.Replace("\0", "");
                if (!IsValidPath(path))  return null;

                return path;
            }
            catch
            {
                return null;
            }
        }

        private string GetExtensionSafe(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                int lastDot = path.LastIndexOf('.');
                int lastSep = path.LastIndexOfAny(new[] { '\\', '/' });

                if (lastDot < 0 || lastDot < lastSep)
                    return null;

                if (lastDot >= path.Length - 1)
                    return null;

                return path.Substring(lastDot);
            }
            catch
            {
                return null;
            }
        }

      
        private string GetFileNameSafe(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                int lastSep = path.LastIndexOfAny(new[] { '\\', '/' });
                if (lastSep < 0)
                    return path;

                if (lastSep >= path.Length - 1)
                    return null;

                return path.Substring(lastSep + 1);
            }
            catch
            {
                return null;
            }
        }

        #endregion Path Validation

        #region IManagedThreadWorker Implementation

        public void Initialize(CancellationToken token)
        {
            try
            {
                _cancellationToken = token;
                _processingTask = Task.Factory.StartNew(ProcessQueueWorker, TaskCreationOptions.LongRunning);
                InitializeWatchers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE RansomwareDetector: Initialize failed: {ex.Message}");
            }
        }

        private void ProcessQueueWorker()
        {
            foreach (var fileEvent in _eventQueue.GetConsumingEnumerable())
            {
                if (_cancellationToken.IsCancellationRequested) break;

                try
                {
                    ProcessFileEventLogic(fileEvent);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing file event: {ex.Message}");
                }
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (_eventQueue.Count < 5000) 
            {
                _eventQueue.TryAdd(new FileEventData { Path = e.FullPath, Type = e.ChangeType });
            }
        }

        
        private void ProcessFileEventLogic(FileEventData e)
        {
            string filePath = e.Path;
            if (!IsValidPath(filePath)) return;
            if (IsSystemPathSafe(filePath)) return;


            int pid = GetWritingProcessIdSafe(filePath);
            if (pid <= 0) return;

            double? entropy = null;
            if (e.Type == WatcherChangeTypes.Changed)
            {
                entropy = CalculateFileEntropySafe(filePath);
            }

            RecordActivitySafe(pid, GetProcessNameSafe(pid), filePath, e.Type, entropy);
        }

        public void Execute()
        {
            _isRunning = true;

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    CleanupOldActivity();
                    CheckObservedProcesses();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHE RansomwareDetector: Loop error: {ex.Message}");
                }

                try
                {
                    Thread.Sleep(1000);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch { }
            }

            _isRunning = false;
        }

        public void Cleanup()
        {
            try
            {
                _isRunning = false;
                DisposeWatchers();

                foreach (var kvp in _suspendedProcesses.ToArray())
                {
                    try
                    {
                        ResumeProcessSafe(kvp.Key);
                    }
                    catch { }
                }

                _suspendedProcesses.Clear();
                _processActivity.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE RansomwareDetector: Cleanup error: {ex.Message}");
            }
        }

        public void OnError(Exception ex)
        {
            try
            {
                _dispatcher?.Enqueue("Ransomware Detection Error",
                    $"Detection service encountered an error: {ex?.Message ?? "Unknown"}");
            }
            catch { }
        }

        #endregion IManagedThreadWorker Implementation

        #region File System Monitoring

        private void InitializeWatchers()
        {
            lock (_watcherLock)
            {
                try
                {
                    var pathsToMonitor = new List<string>();

                    try
                    {
                        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        if (!string.IsNullOrEmpty(userProfile) && Directory.Exists(userProfile))
                            pathsToMonitor.Add(userProfile);
                    }
                    catch { }

                    try
                    {
                        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        if (!string.IsNullOrEmpty(docs) && Directory.Exists(docs))
                            pathsToMonitor.Add(docs);
                    }
                    catch { }

                    try
                    {
                        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        if (!string.IsNullOrEmpty(desktop) && Directory.Exists(desktop))
                            pathsToMonitor.Add(desktop);
                    }
                    catch { }

                    try
                    {
                        foreach (var drive in DriveInfo.GetDrives())
                        {
                            try
                            {
                                if (drive.DriveType == DriveType.Fixed && drive.IsReady)
                                {
                                    pathsToMonitor.Add(drive.RootDirectory.FullName);
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }

                    foreach (var path in pathsToMonitor.Distinct())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                                continue;

                            var watcher = new FileSystemWatcher(path)
                            {
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                                IncludeSubdirectories = true,
                                EnableRaisingEvents = true,
                                InternalBufferSize = 65536
                            };

                            watcher.Changed += OnFileChangedSafe;
                            watcher.Created += OnFileChangedSafe;
                            watcher.Renamed += OnFileRenamedSafe;
                            watcher.Error += OnWatcherErrorSafe;

                            _watchers.Add(watcher);
                            Debug.WriteLine($"LHE: Watcher created for {path}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"LHE: Failed to create watcher for {path}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHE: InitializeWatchers failed: {ex.Message}");
                }
            }
        }

        private void OnFileChangedSafe(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (_cancellationToken.IsCancellationRequested || !_isRunning || _disposed)
                    return;

                if (e == null)
                    return;
                var safePath = SanitizePath(e.FullPath);
                if (safePath == null)
                    return;

                ProcessFileEventSafe(safePath, e.ChangeType);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: OnFileChangedSafe error: {ex.Message}");
            }
        }

        private void OnFileRenamedSafe(object sender, RenamedEventArgs e)
        {
            try
            {
                if (_cancellationToken.IsCancellationRequested || !_isRunning || _disposed)
                    return;

                if (e == null)
                    return;

                var safePath = SanitizePath(e.FullPath);
                if (safePath == null)
                    return;

                ProcessFileEventSafe(safePath, WatcherChangeTypes.Renamed);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: OnFileRenamedSafe error: {ex.Message}");
            }
        }

        private void OnWatcherErrorSafe(object sender, ErrorEventArgs e)
        {
            try
            {
                var ex = e?.GetException();
                Debug.WriteLine($"LHE: FileSystemWatcher error: {ex?.Message ?? "Unknown"}");
            }
            catch { }
        }

        private void ProcessFileEventSafe(string filePath, WatcherChangeTypes changeType)
        {
            try
            {
                
                string ext = GetExtensionSafe(filePath);
                if (string.IsNullOrEmpty(ext))
                    return;

                if (!MonitoredExtensions.Contains(ext))
                    return;

                if (IsSystemPathSafe(filePath))
                    return;

               
                int lockingProcessId = GetWritingProcessIdSafe(filePath);
                if (lockingProcessId <= 0)
                    return;


                string processName = GetProcessNameSafe(lockingProcessId);
                if (string.IsNullOrEmpty(processName))
                    return;

                if (SafeProcesses.Contains(processName))
                    return;


                lock (_whitelistLock)
                {
                    if (_sessionWhitelist.Contains(processName))
                        return;
                }


                double? entropy = null;
                if (changeType == WatcherChangeTypes.Changed)
                {
                    entropy = CalculateFileEntropySafe(filePath);
                }


                RecordActivitySafe(lockingProcessId, processName, filePath, changeType, entropy);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: ProcessFileEventSafe error: {ex.Message}");
            }
        }

        #endregion File System Monitoring

        #region Activity Tracking and Detection

        private void RecordActivitySafe(int processId, string processName, string filePath,
            WatcherChangeTypes changeType, double? entropy)
        {
            try
            {
                var activity = _processActivity.GetOrAdd(processId, pid =>
                {
                    try
                    {
                        return new ProcessActivity
                        {
                            ProcessName = processName ?? "Unknown",
                            ProcessPath = GetProcessPathSafe(pid) ?? "Unknown"
                        };
                    }
                    catch
                    {
                        return new ProcessActivity
                        {
                            ProcessName = processName ?? "Unknown",
                            ProcessPath = "Unknown"
                        };
                    }
                });

                if (activity == null || activity.IsBlocked)
                    return;

                var mod = new FileModification(filePath, changeType, entropy);
                activity.Modifications.Enqueue(mod);
                Interlocked.Increment(ref activity.TotalModifications);

                AnalyzeActivitySafe(processId, activity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: RecordActivitySafe error: {ex.Message}");
            }
        }

        private void AnalyzeActivitySafe(int processId, ProcessActivity activity)
        {
            try
            {
                if (activity == null || activity.IsBlocked)
                    return;

                var now = DateTime.UtcNow;
                var windowStart = now.AddMilliseconds(-OBSERVATION_WINDOW_MS);

                List<FileModification> recentMods;
                try
                {
                    recentMods = activity.Modifications
                        .Where(m => m.Timestamp >= windowStart)
                        .ToList();
                }
                catch
                {
                    return;
                }

                var recentCount = recentMods.Count;
                var highEntropyCount = 0;

                try
                {
                    highEntropyCount = recentMods.Count(m => m.Entropy.HasValue && m.Entropy.Value >= HIGH_ENTROPY_THRESHOLD);
                }
                catch { }

                if (recentCount >= IMMEDIATE_BLOCK_THRESHOLD || highEntropyCount >= IMMEDIATE_BLOCK_THRESHOLD / 2)
                {
                    HandleThreatDetectedSafe(processId, activity, recentMods, true);
                    return;
                }

                if (recentCount >= OBSERVATION_THRESHOLD && !activity.IsUnderObservation)
                {
                    activity.IsUnderObservation = true;
                    activity.ObservationStarted = DateTime.UtcNow;

                    try
                    {
                        _dispatcher?.Enqueue("Suspicious Activity Detected",
                            $"Process '{activity.ProcessName}' is modifying files rapidly. Monitoring...");
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: AnalyzeActivitySafe error: {ex.Message}");
            }
        }

        private void CheckObservedProcesses()
        {
            try
            {
                foreach (var kvp in _processActivity.ToArray())
                {
                    try
                    {
                        var activity = kvp.Value;
                        if (activity == null || !activity.IsUnderObservation || activity.IsBlocked)
                            continue;

                        if (activity.TotalModifications >= SUSTAINED_BLOCK_THRESHOLD)
                        {
                            List<FileModification> recentMods;
                            try
                            {
                                recentMods = activity.Modifications.ToList();
                            }
                            catch
                            {
                                recentMods = new List<FileModification>();
                            }

                            HandleThreatDetectedSafe(kvp.Key, activity, recentMods, false);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: CheckObservedProcesses error: {ex.Message}");
            }
        }

        private void HandleThreatDetectedSafe(int processId, ProcessActivity activity,
            List<FileModification> modifications, bool isImmediate)
        {
            try
            {
                if (activity == null || activity.IsBlocked)
                    return;

                activity.IsBlocked = true;

                try
                {
                    SuspendProcessSafe(processId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LHE: Failed to suspend process: {ex.Message}");
                }

                SuspendedProcessInfo suspendedInfo;
                try
                {
                    var affectedFiles = new List<string>();
                    try
                    {
                        if (modifications != null)
                        {
                            affectedFiles = modifications
                                .Where(m => !string.IsNullOrEmpty(m.FilePath))
                                .Select(m => m.FilePath)
                                .Distinct()
                                .Take(10)
                                .ToList();
                        }
                    }
                    catch { }

                    suspendedInfo = new SuspendedProcessInfo
                    {
                        ProcessId = processId,
                        ProcessName = activity.ProcessName ?? "Unknown",
                        ProcessPath = activity.ProcessPath ?? "Unknown",
                        SuspendedAt = DateTime.UtcNow,
                        ModificationCount = modifications?.Count ?? 0,
                        AffectedFiles = affectedFiles
                    };
                }
                catch
                {
                    suspendedInfo = new SuspendedProcessInfo
                    {
                        ProcessId = processId,
                        ProcessName = "Unknown",
                        ProcessPath = "Unknown",
                        SuspendedAt = DateTime.UtcNow,
                        ModificationCount = 0,
                        AffectedFiles = new List<string>()
                    };
                }

                _suspendedProcesses.TryAdd(processId, suspendedInfo);

              
                try
                {
                    _dispatcher?.Enqueue("Ransomware Activity Blocked!",
                        $"Process '{suspendedInfo.ProcessName}' (PID: {processId}) was suspended.\n" +
                        $"Modified {suspendedInfo.ModificationCount} files.\n" +
                        "Check the popup to allow or terminate.");
                }
                catch { }

                ShowDecisionDialogSafe(suspendedInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: HandleThreatDetectedSafe error: {ex.Message}");
            }
        }

        private void ShowDecisionDialogSafe(SuspendedProcessInfo info)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        ShowDecisionDialogInternalSafe(info);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LHE: Dialog thread error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: ShowDecisionDialogSafe error: {ex.Message}");
            }
        }

        private void ShowDecisionDialogInternalSafe(SuspendedProcessInfo info)
        {
            try
            {
                if (info == null)
                    return;

                string affectedList = "";
                try
                {
                    if (info.AffectedFiles != null && info.AffectedFiles.Count > 0)
                    {
                        affectedList = string.Join("\n", info.AffectedFiles.Take(5));
                        if (info.AffectedFiles.Count > 5)
                            affectedList += $"\n... and {info.AffectedFiles.Count - 5} more";
                    }
                }
                catch
                {
                    affectedList = "Unable to retrieve file list";
                }

                var message = $"POTENTIAL RANSOMWARE DETECTED!\n\n" +
                             $"Process: {info.ProcessName ?? "Unknown"}\n" +
                             $"Path: {info.ProcessPath ?? "Unknown"}\n" +
                             $"PID: {info.ProcessId}\n" +
                             $"Files Modified: {info.ModificationCount}\n\n" +
                             $"Affected Files:\n{affectedList}\n\n" +
                             $"Do you want to ALLOW this process to continue?\n" +
                             $"Click 'Yes' to resume, 'No' to terminate the process.";

                DialogResult result;
                try
                {
                    result = MessageBox.Show(
                        message,
                        "SecVerse LHE - Ransomware Alert",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                }
                catch
                {
                    result = DialogResult.No;
                }

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        ResumeProcessSafe(info.ProcessId);
                        _suspendedProcesses.TryRemove(info.ProcessId, out _);

                        if (!string.IsNullOrEmpty(info.ProcessName))
                        {
                            lock (_whitelistLock)
                            {
                                try
                                {
                                    _sessionWhitelist.Add(info.ProcessName.ToLowerInvariant());
                                }
                                catch { }
                            }
                        }

                        _dispatcher?.Enqueue("Process Resumed",
                            $"'{info.ProcessName}' has been allowed to continue.");
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        TerminateProcessSafe(info.ProcessId);
                        _suspendedProcesses.TryRemove(info.ProcessId, out _);

                        _dispatcher?.Enqueue("Process Terminated",
                            $"'{info.ProcessName}' has been terminated as potential ransomware.");
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: ShowDecisionDialogInternalSafe error: {ex.Message}");
            }
        }

        private void CleanupOldActivity()
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddSeconds(-30);

                foreach (var kvp in _processActivity.ToArray())
                {
                    try
                    {
                        var activity = kvp.Value;
                        if (activity == null)
                        {
                            _processActivity.TryRemove(kvp.Key, out _);
                            continue;
                        }

                        try
                        {
                            while (activity.Modifications.TryPeek(out var mod) && mod.Timestamp < cutoff)
                            {
                                activity.Modifications.TryDequeue(out _);
                            }
                        }
                        catch { }

                        if (!activity.IsUnderObservation && !activity.IsBlocked && activity.Modifications.IsEmpty)
                        {
                            _processActivity.TryRemove(kvp.Key, out _);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: CleanupOldActivity error: {ex.Message}");
            }
        }

        #endregion Activity Tracking and Detection

        #region Process Control - Native APIs

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSuspendProcess(IntPtr processHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtResumeProcess(IntPtr processHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr processHandle, uint exitCode);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags,
            StringBuilder lpExeName, ref int lpdwSize);

        private const uint PROCESS_SUSPEND_RESUME = 0x0800;
        private const uint PROCESS_TERMINATE = 0x0001;
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        #endregion Process Control - Native APIs

        #region Safe Process Operations

        private bool IsValidHandle(IntPtr handle)
        {
            return handle != IntPtr.Zero && handle != new IntPtr(-1);
        }

        private void SuspendProcessSafe(int processId)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = OpenProcess(PROCESS_SUSPEND_RESUME, false, processId);
                if (!IsValidHandle(handle))
                {
                    Debug.WriteLine($"LHE: Cannot open process {processId} for suspend");
                    return;
                }

                var result = NtSuspendProcess(handle);
                if (result != 0)
                {
                    Debug.WriteLine($"LHE: NtSuspendProcess failed with {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: SuspendProcessSafe error: {ex.Message}");
            }
            finally
            {
                if (IsValidHandle(handle))
                {
                    try { CloseHandle(handle); } catch { }
                }
            }
        }

        private void ResumeProcessSafe(int processId)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = OpenProcess(PROCESS_SUSPEND_RESUME, false, processId);
                if (!IsValidHandle(handle))
                    return;

                NtResumeProcess(handle);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: ResumeProcessSafe error: {ex.Message}");
            }
            finally
            {
                if (IsValidHandle(handle))
                {
                    try { CloseHandle(handle); } catch { }
                }
            }
        }

        private void TerminateProcessSafe(int processId)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = OpenProcess(PROCESS_TERMINATE, false, processId);
                if (!IsValidHandle(handle))
                    return;

                TerminateProcess(handle, 1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: TerminateProcessSafe error: {ex.Message}");
            }
            finally
            {
                if (IsValidHandle(handle))
                {
                    try { CloseHandle(handle); } catch { }
                }
            }
        }

        private string GetProcessNameSafe(int processId)
        {
            try
            {
                if (processId <= 0)
                    return null;

                using (var process = Process.GetProcessById(processId))
                {
                    return process?.ProcessName;
                }
            }
            catch
            {
                return null;
            }
        }

        private string GetProcessPathSafe(int processId)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                if (processId <= 0)
                    return null;

                handle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
                if (!IsValidHandle(handle))
                    return null;

                var buffer = new StringBuilder(1024);
                var size = buffer.Capacity;
                if (QueryFullProcessImageName(handle, 0, buffer, ref size))
                {
                    return buffer.ToString();
                }
            }
            catch { }
            finally
            {
                if (IsValidHandle(handle))
                {
                    try { CloseHandle(handle); } catch { }
                }
            }

            return null;
        }

        private int GetWritingProcessIdSafe(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return -1;

                string fileName = GetFileNameSafe(filePath)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(fileName))
                    return -1;

                Process[] processes = null;
                try
                {
                    processes = Process.GetProcesses();
                }
                catch { return -1; }

                if (processes == null)
                    return -1;

                try
                {
                    foreach (var proc in processes)
                    {
                        try
                        {
                            if (proc.Id <= 4)
                                continue;

                            var procName = proc.ProcessName?.ToLowerInvariant();
                            if (string.IsNullOrEmpty(procName))
                                continue;

                            if (SafeProcesses.Contains(procName))
                                continue;

                            try
                            {
                                var startTime = proc.StartTime;
                                if ((DateTime.Now - startTime).TotalSeconds < 30)
                                {
                                    return proc.Id;
                                }
                            }
                            catch { }
                        }
                        catch { }
                    }
                }
                finally
                {
                    // Alle Prozesse aufräumen
                    if (processes != null)
                    {
                        foreach (var proc in processes)
                        {
                            try { proc?.Dispose(); } catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LHE: GetWritingProcessIdSafe error: {ex.Message}");
            }

            return -1;
        }

        #endregion Safe Process Operations

        #region Utility Methods

        private bool IsSystemPathSafe(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return true;

                var lowerPath = path.ToLowerInvariant();
                return lowerPath.Contains("\\windows\\") ||
                       lowerPath.Contains("\\program files") ||
                       lowerPath.Contains("\\programdata\\") ||
                       lowerPath.Contains("\\appdata\\local\\temp\\") ||
                       lowerPath.Contains("\\$recycle.bin\\") ||
                       lowerPath.Contains("\\.git\\") ||
                       lowerPath.Contains("\\.vs\\") ||
                       lowerPath.Contains("\\node_modules\\") ||
                       lowerPath.Contains("\\obj\\") ||
                       lowerPath.Contains("\\bin\\debug\\") ||
                       lowerPath.Contains("\\bin\\release\\") ||
                       lowerPath.Contains("\\packages\\") ||
                       lowerPath.Contains("\\.nuget\\");
            }
            catch
            {
                return true;
            }
        }

        private double CalculateFileEntropySafe(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath)) return 0;
                if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return 0;
                try
                {
                    if (!File.Exists(filePath)) return 0;

                    var bytes = new byte[ENTROPY_SAMPLE_SIZE];
                    int bytesRead = 0;

                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                           FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.SequentialScan))
                    {
                        bytesRead = fs.Read(bytes, 0, ENTROPY_SAMPLE_SIZE);
                    }

                    if (bytesRead == 0) return 0;

                    var frequency = new int[256];
                    for (int i = 0; i < bytesRead; i++) frequency[bytes[i]]++;

                    double entropy = 0;
                    double logBase = Math.Log(2);
                    for (int i = 0; i < 256; i++)
                    {
                        if (frequency[i] > 0)
                        {
                            double p = (double)frequency[i] / bytesRead;
                            entropy -= p * (Math.Log(p) / logBase);
                        }
                    }
                    return entropy;
                }
                catch (ArgumentException) { return 0; }
                catch (NotSupportedException) { return 0; } 
                catch (IOException) { return 0; } 
                catch (UnauthorizedAccessException) { return 0; }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Entropy calc failed catastrophic: {ex.Message}");
                return 0;
            }
        }

        private void DisposeWatchers()
        {
            lock (_watcherLock)
            {
                try
                {
                    foreach (var watcher in _watchers.ToArray())
                    {
                        try
                        {
                            if (watcher != null)
                            {
                                watcher.EnableRaisingEvents = false;
                                watcher.Changed -= OnFileChangedSafe;
                                watcher.Created -= OnFileChangedSafe;
                                watcher.Renamed -= OnFileRenamedSafe;
                                watcher.Error -= OnWatcherErrorSafe;
                                watcher.Dispose();
                            }
                        }
                        catch { }
                    }
                    _watchers.Clear();
                }
                catch { }
            }
        }

        #endregion Utility Methods

        #region IDisposable

        public void Dispose()
        {
            try
            {
                if (_disposed)
                    return;

                _disposed = true;
                Cleanup();
            }
            catch { }
        }

        #endregion IDisposable

        private class FileEventData
        {
            public string Path { get; set; }
            public WatcherChangeTypes Type { get; set; }
        }

    }
}
