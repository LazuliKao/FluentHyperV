using Wpf.Ui.Abstractions.Controls;

namespace FluentHyperV.Desktop.ViewModels.Pages;

public class VirtualMachinesViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;

    public async Task OnNavigatedToAsync()
    {
        var api = new HyperV.HyperVApi();
        var vm = await api.GetVMAsync(new());
        if (!_isInitialized)
        {
            // InitializeViewModel();
            _isInitialized = true;
        }
        await Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
