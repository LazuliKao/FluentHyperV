using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Abstractions.Controls;

namespace FluentHyperV.Desktop.ViewModels.Pages;

public partial class VirtualMachineSettingsViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;
    
    [ObservableProperty]
    private string _virtualMachineName = string.Empty;
    
    [ObservableProperty]
    private string _virtualMachineId = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    // 硬件设置
    [ObservableProperty]
    private long _memoryStartupMB = 2048;

    [ObservableProperty]
    private bool _dynamicMemoryEnabled = true;

    [ObservableProperty]
    private long _minimumMemoryMB = 512;

    [ObservableProperty]
    private long _maximumMemoryMB = 4096;

    [ObservableProperty]
    private int _processorCount = 2;

    [ObservableProperty]
    private bool _enableNestedVirtualization = false;

    [ObservableProperty]
    private int _virtualMachineGeneration = 2;

    [ObservableProperty]
    private bool _enableSecureBoot = true;

    // 管理设置
    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _enableIntegrationServices = true;

    [ObservableProperty]
    private string _smartPagingFileLocation = string.Empty;

    [ObservableProperty]
    private string _automaticStartAction = "Restart";

    [ObservableProperty]
    private string _automaticStopAction = "Save";

    [ObservableProperty]
    private ObservableCollection<string> _startActionOptions = new() { "Nothing", "Start", "Restart" };

    [ObservableProperty]
    private ObservableCollection<string> _stopActionOptions = new() { "Save", "TurnOff", "Shutdown" };

    public VirtualMachineSettingsViewModel()
    {
    }

    public async Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
        {
            await LoadVirtualMachineSettingsAsync();
            _isInitialized = true;
        }
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    public void SetVirtualMachine(string vmName, string vmId)
    {
        VirtualMachineName = vmName;
        VirtualMachineId = vmId;
    }

    [RelayCommand]
    private async Task LoadVirtualMachineSettingsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在加载虚拟机设置...";

            // 这里应该调用 HyperV API 来加载实际的设置
            // 目前先模拟加载
            await Task.Delay(1000);

            StatusMessage = "设置加载完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载设置失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在保存设置...";

            // 这里应该调用 HyperV API 来保存设置
            await Task.Delay(1000);

            StatusMessage = "设置保存成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存设置失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        // 返回到虚拟机列表页面
        // 这里需要通过导航服务来实现
    }
}
