using System.Collections.ObjectModel;
using System.Windows.Input;
using Facet;
using Microsoft.HyperV.PowerShell;
using Wpf.Ui.Abstractions.Controls;
using FluentHyperV.HyperV;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentHyperV.Desktop.ViewModels.Pages;

[DependencyInjectionTransient]
[Facet(typeof(VirtualMachine))]
public partial class VirtualMachineViewModel : ObservableObject { }

public partial class VirtualMachinesViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;
    private readonly HyperVApi _hyperVApi;

    [ObservableProperty]
    private ObservableCollection<VirtualMachine> virtualMachines = new();

    [ObservableProperty]
    private VirtualMachine? selectedVirtualMachine;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public VirtualMachinesViewModel()
    {
        _hyperVApi = new HyperVApi();
    }

    public async Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
        {
            await LoadVirtualMachinesAsync();
            _isInitialized = true;
        }
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    [RelayCommand]
    private async Task LoadVirtualMachinesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "加载虚拟机列表...";
            
            var vmArray = await _hyperVApi.GetVMAsync(new HyperVApi.GetVMArguments());
            
            VirtualMachines.Clear();
            foreach (var vm in vmArray)
            {
                VirtualMachines.Add(vm);
            }
            
            StatusMessage = $"已加载 {VirtualMachines.Count} 台虚拟机";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载虚拟机失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartVirtualMachineAsync()
    {
        if (SelectedVirtualMachine == null) return;

        try
        {
            StatusMessage = $"正在启动虚拟机 {SelectedVirtualMachine.Name}...";
            // TODO: 实现启动虚拟机的逻辑
            await Task.Delay(1000); // 模拟操作
            StatusMessage = $"虚拟机 {SelectedVirtualMachine.Name} 启动成功";
            await LoadVirtualMachinesAsync(); // 刷新状态
        }
        catch (Exception ex)
        {
            StatusMessage = $"启动虚拟机失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StopVirtualMachineAsync()
    {
        if (SelectedVirtualMachine == null) return;

        try
        {
            StatusMessage = $"正在停止虚拟机 {SelectedVirtualMachine.Name}...";
            // TODO: 实现停止虚拟机的逻辑
            await Task.Delay(1000); // 模拟操作
            StatusMessage = $"虚拟机 {SelectedVirtualMachine.Name} 停止成功";
            await LoadVirtualMachinesAsync(); // 刷新状态
        }
        catch (Exception ex)
        {
            StatusMessage = $"停止虚拟机失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RestartVirtualMachineAsync()
    {
        if (SelectedVirtualMachine == null) return;

        try
        {
            StatusMessage = $"正在重启虚拟机 {SelectedVirtualMachine.Name}...";
            // TODO: 实现重启虚拟机的逻辑
            await Task.Delay(1000); // 模拟操作
            StatusMessage = $"虚拟机 {SelectedVirtualMachine.Name} 重启成功";
            await LoadVirtualMachinesAsync(); // 刷新状态
        }
        catch (Exception ex)
        {
            StatusMessage = $"重启虚拟机失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadVirtualMachinesAsync();
    }
}
