using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TheWatcher.Data.Models
{
    public class DuplicateGroup
    {
        public string FileHash { get; set; }
        public long SizeBytes { get; set; }

        public List<FileTask> Files { get; set; } = new();

        public void AutoSelectKeepOldest()
        {
            if (Files == null || Files.Count == 0) return;

            var oldestFile = Files.OrderBy(f => f.CreatedDate).First();
            foreach (var file in Files)
            {
                if (file == oldestFile)
                {
                    file.DuplicateState = DuplicatedState.Primary;
                }

                else
                {
                    file.DuplicateState = DuplicatedState.Redundant;
                }
            }
        }
    }
}
