using TheWatcher.Interfaces;

namespace TheWatcher.Services
{
    public class FolderPickerService : IFolderPickerService
    {
        public async Task<string?> PickFolderAsync()
        {
#if WINDOWS
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            var mauiWindow = Application.Current?.Windows[0].Handler?.PlatformView;
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(mauiWindow);

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, windowHandle);

            var result = await folderPicker.PickSingleFolderAsync();

            return result?.Path;
#else
            return null;
#endif
        }
    }
}
