using TheWatcher.Data.Models;

namespace TheWatcher.Interfaces
{
    public interface IOrganizerEngine
    {
        FileTask ProcessFile(FileTask task, List<OrganizerRuler> ruler);
        Task<FileExecutionLog> ExecuteTaskAsync(FileTask task);
    }
}
