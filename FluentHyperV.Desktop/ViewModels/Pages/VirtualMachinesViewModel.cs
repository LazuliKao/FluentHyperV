using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Facet;
using Facet.Mapping;
using FluentHyperV.HyperV;
using Microsoft.HyperV.PowerShell;
using Wpf.Ui.Abstractions.Controls;

namespace FluentHyperV.Desktop.ViewModels.Pages;

[DependencyInjectionTransient]
[Facet(typeof(VirtualMachine))]
public partial class VirtualMachineViewModel : ObservableObject { }

public partial class VirtualMachinesViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;
    private readonly HyperVApi _hyperVApi = new();

    [ObservableProperty]
    private ObservableCollection<VirtualMachineViewModel> virtualMachines = new();

    [ObservableProperty]
    private VirtualMachineViewModel? selectedVirtualMachine;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

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
                VirtualMachines.Add(VirtualMachineViewModel.Projection.Compile().Invoke(vm));
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
        if (SelectedVirtualMachine == null)
            return;

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
        if (SelectedVirtualMachine == null)
            return;

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
        if (SelectedVirtualMachine == null)
            return;

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

    [RelayCommand]
    private async Task PauseVirtualMachineAsync()
    {
        if (SelectedVirtualMachine == null)
            return;

        try
        {
            StatusMessage = $"正在暂停虚拟机 {SelectedVirtualMachine.Name}...";
            // TODO: 实现暂停虚拟机的逻辑
            await Task.Delay(1000); // 模拟操作
            StatusMessage = $"虚拟机 {SelectedVirtualMachine.Name} 暂停成功";
            await LoadVirtualMachinesAsync(); // 刷新状态
        }
        catch (Exception ex)
        {
            StatusMessage = $"暂停虚拟机失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ResumeVirtualMachineAsync()
    {
        if (SelectedVirtualMachine == null)
            return;

        try
        {
            StatusMessage = $"正在恢复虚拟机 {SelectedVirtualMachine.Name}...";
            // TODO: 实现恢复虚拟机的逻辑
            await Task.Delay(1000); // 模拟操作
            StatusMessage = $"虚拟机 {SelectedVirtualMachine.Name} 恢复成功";
            await LoadVirtualMachinesAsync(); // 刷新状态
        }
        catch (Exception ex)
        {
            StatusMessage = $"恢复虚拟机失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateCheckpointAsync()
    {
        if (SelectedVirtualMachine == null)
            return;

        try
        {
            StatusMessage = $"正在为虚拟机 {SelectedVirtualMachine.Name} 创建检查点...";
            // TODO: 实现创建检查点的逻辑
            await Task.Delay(2000); // 模拟较长的操作
            StatusMessage = $"虚拟机 {SelectedVirtualMachine.Name} 检查点创建成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"创建检查点失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private Task ConnectToVirtualMachineAsync()
    {
        if (SelectedVirtualMachine == null)
            return Task.CompletedTask;

        try
        {
            StatusMessage = $"正在连接到虚拟机 {SelectedVirtualMachine.Name}...";

            // 启动虚拟机连接程序
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "vmconnect.exe",
                Arguments = $"localhost \"{SelectedVirtualMachine.Name}\"",
                UseShellExecute = true,
            };

            System.Diagnostics.Process.Start(processInfo);
            StatusMessage = $"已启动到虚拟机 {SelectedVirtualMachine.Name} 的连接";
        }
        catch (Exception ex)
        {
            StatusMessage = $"连接虚拟机失败: {ex.Message}";
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ShowVirtualMachineSettingsAsync()
    {
        if (SelectedVirtualMachine == null)
            return;

        try
        {
            StatusMessage = $"正在打开虚拟机 {SelectedVirtualMachine.Name} 的设置...";
            // TODO: 实现显示虚拟机设置对话框的逻辑
            await Task.Delay(500); // 模拟操作
            StatusMessage = "设置功能暂未实现";
        }
        catch (Exception ex)
        {
            StatusMessage = $"打开设置失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteVirtualMachineAsync()
    {
        if (SelectedVirtualMachine == null)
            return;

        try
        {
            // 这里应该显示确认对话框
            var result = MessageBox.Show(
                $"确定要删除虚拟机 '{SelectedVirtualMachine.Name}' 吗？此操作无法撤销。",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                StatusMessage = $"正在删除虚拟机 {SelectedVirtualMachine.Name}...";
                // TODO: 实现删除虚拟机的逻辑
                await Task.Delay(1000); // 模拟操作
                StatusMessage = $"虚拟机 {SelectedVirtualMachine.Name} 删除成功";
                await LoadVirtualMachinesAsync(); // 刷新列表
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除虚拟机失败: {ex.Message}";
        }
    }
}
