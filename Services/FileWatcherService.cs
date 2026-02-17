using System;
using System.Collections.Generic;
using System.Text;
using TheWatcher.Data.Models;

namespace TheWatcher.Services
{
    public class FileWatcherService : IDisposable
    {
        private readonly Dictionary<string, FileSystemWatcher> _activeWathers = new();

        public event Action<FileTask>? OnFileDetected;

        public void StartWatching(List<OrganizerRuler> activeRules)
        {
            var foldersToWatch = activeRules
                .Select(r => r.SourceDirectory)
                .Distinct()
                .Where(path => Directory.Exists(path))
                .ToList();

            foreach (var path in foldersToWatch)
            {
                if (_activeWathers.ContainsKey(path)) continue;

                RegisterWatcher(path);
            }
        }

        public void RegisterWatcher(string path)
        {
            try
            {
                var watcher = new FileSystemWatcher(path);

                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;

                watcher.Filter = "*.*";

                watcher.IncludeSubdirectories = true;
                watcher.Created += OnFileCreated;
                watcher.Renamed += OnFileRenamed;

                watcher.EnableRaisingEvents = true;
                _activeWathers.Add(path, watcher);
                Console.WriteLine($"Started Watching : {path}");
            }
            catch (Exception ex) { 
                Console.WriteLine($"Failed to watch {path}: {ex.Message}");
            }
        }

        // handlers

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            BroadcastFile(e.FullPath);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            BroadcastFile(e.FullPath);
        }

        private void BroadcastFile(string fullPath)
        {
            var fileInfo = new FileInfo(fullPath);

            if (!fileInfo.Exists) return;

            var task = new FileTask
            {
                OriginalFullPath = fullPath,
                FileName = fileInfo.Name,
                Status = Data.Models.TaskStatus.Pending,
                CreatedDate = DateTime.Now
            };

            OnFileDetected?.Invoke(task);
        }


        public void StopAll()
        {
            foreach (var watcher in _activeWathers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            _activeWathers.Clear();
        }

        public void Dispose()
        {
            StopAll();
        }
    }
}
