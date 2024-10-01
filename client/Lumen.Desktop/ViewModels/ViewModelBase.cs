using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Lumen.Desktop.ViewModels;

public class ViewModelBase : ObservableObject
{
}

public abstract class StorageBasedViewModel : ViewModelBase
{
    protected StorageBasedViewModel()
    {
        PropertyChanged += (_, _) =>
        {
            UpdateStorageValues();
            WeakReferenceMessenger.Default.Send(new SaveStateMessage());
        };
    }

    protected abstract void UpdateStorageValues();
}