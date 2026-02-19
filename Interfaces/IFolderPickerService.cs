namespace TheWatcher.Interfaces
{
    public interface IFolderPickerService
    {
        Task<string?> PickFolderAsync();
    }
}
