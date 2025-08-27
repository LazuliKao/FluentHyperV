using System.Collections.ObjectModel;
using Facet;
using Microsoft.HyperV.PowerShell;
using Wpf.Ui.Abstractions.Controls;
using FluentHyperV.PowerShell;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentHyperV.Desktop.ViewModels.Pages;

[DependencyInjectionTransient]
[Facet(typeof(VMSwitch))]
public partial class VirtualSwitchViewModel : ObservableObject { }

[DependencyInjectionTransient]
public partial class VirtualSwitchesViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;
    private readonly HyperVInstance _hyperVInstance;

    [ObservableProperty]
    private ObservableCollection<VMSwitch> _virtualSwitches = new();

    [ObservableProperty]
    private VMSwitch? _selectedVirtualSwitch;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _newSwitchName = string.Empty;

    [ObservableProperty]
    private string _selectedSwitchType = "External";

    [ObservableProperty]
    private bool _isCreateDialogOpen;

    // 详细信息编辑相关属性
    [ObservableProperty]
    private string _editingSwitchName = string.Empty;

    [ObservableProperty]
    private string _editingSwitchNotes = string.Empty;

    [ObservableProperty]
    private bool _isEditingDetails;

    [ObservableProperty]
    private bool _hasSelectedSwitch;

    public ObservableCollection<string> SwitchTypes { get; } = new()
    {
        "External",
        "Internal", 
        "Private"
    };

    public VirtualSwitchesViewModel()
    {
        _hyperVInstance = new HyperVInstance();
    }

    // 当选中的虚拟交换机发生变化时的处理
    partial void OnSelectedVirtualSwitchChanged(VMSwitch? value)
    {
        HasSelectedSwitch = value != null;
        if (value != null)
        {
            EditingSwitchName = value.Name;
            EditingSwitchNotes = value.Notes ?? string.Empty;
            IsEditingDetails = false;
        }
        else
        {
            EditingSwitchName = string.Empty;
            EditingSwitchNotes = string.Empty;
            IsEditingDetails = false;
        }
    }

    public async Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
        {
            await LoadVirtualSwitchesAsync();
            _isInitialized = true;
        }
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    [RelayCommand]
    private async Task LoadVirtualSwitchesAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载虚拟交换机...";

        try
        {
            var switches = await Task.Run(() => _hyperVInstance.InvokeFunction<VMSwitch>("Get-VMSwitch", new Dictionary<string, object>()));
            
            VirtualSwitches.Clear();
            foreach (var vmSwitch in switches)
            {
                VirtualSwitches.Add(vmSwitch);
            }

            StatusMessage = $"共加载 {VirtualSwitches.Count} 个虚拟交换机";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载虚拟交换机失败: {ex.Message}";
            MessageBox.Show($"加载虚拟交换机失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenCreateDialog()
    {
        NewSwitchName = string.Empty;
        SelectedSwitchType = "External";
        IsCreateDialogOpen = true;
    }

    [RelayCommand]
    private void CloseCreateDialog()
    {
        IsCreateDialogOpen = false;
    }

    [RelayCommand]
    private async Task CreateVirtualSwitchAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSwitchName))
        {
            MessageBox.Show("请输入虚拟交换机名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "正在创建虚拟交换机...";

            var parameters = new Dictionary<string, object>
            {
                ["Name"] = NewSwitchName,
                ["SwitchType"] = SelectedSwitchType
            };

            await Task.Run(() => _hyperVInstance.InvokeFunction<object>("New-VMSwitch", parameters));
            
            IsCreateDialogOpen = false;
            StatusMessage = "虚拟交换机创建成功";
            
            await LoadVirtualSwitchesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"创建虚拟交换机失败: {ex.Message}";
            MessageBox.Show($"创建虚拟交换机失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteVirtualSwitchAsync()
    {
        if (SelectedVirtualSwitch == null)
        {
            MessageBox.Show("请选择要删除的虚拟交换机", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"确定要删除虚拟交换机 '{SelectedVirtualSwitch.Name}' 吗？",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "正在删除虚拟交换机...";

            var parameters = new Dictionary<string, object>
            {
                ["Name"] = SelectedVirtualSwitch.Name
            };

            await Task.Run(() => _hyperVInstance.InvokeFunction<object>("Remove-VMSwitch", parameters));
            
            StatusMessage = "虚拟交换机删除成功";
            await LoadVirtualSwitchesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除虚拟交换机失败: {ex.Message}";
            MessageBox.Show($"删除虚拟交换机失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadVirtualSwitchesAsync();
    }

    [RelayCommand]
    private void ViewSwitchDetails()
    {
        if (SelectedVirtualSwitch == null)
        {
            MessageBox.Show("请选择要查看的虚拟交换机", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var details = $"名称: {SelectedVirtualSwitch.Name}\n" +
                     $"类型: {SelectedVirtualSwitch.SwitchType}\n" +
                     $"ID: {SelectedVirtualSwitch.Id}\n" +
                     $"备注: {SelectedVirtualSwitch.Notes ?? "无"}";

        MessageBox.Show(details, "虚拟交换机详情", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void StartEditDetails()
    {
        if (SelectedVirtualSwitch != null)
        {
            IsEditingDetails = true;
        }
    }

    [RelayCommand]
    private void CancelEditDetails()
    {
        if (SelectedVirtualSwitch != null)
        {
            EditingSwitchName = SelectedVirtualSwitch.Name;
            EditingSwitchNotes = SelectedVirtualSwitch.Notes ?? string.Empty;
            IsEditingDetails = false;
        }
    }

    [RelayCommand]
    private async Task SaveEditDetailsAsync()
    {
        if (SelectedVirtualSwitch == null || string.IsNullOrWhiteSpace(EditingSwitchName))
        {
            MessageBox.Show("名称不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "正在保存更改...";

            // 如果名称发生了变化，需要使用Rename-VMSwitch
            if (EditingSwitchName != SelectedVirtualSwitch.Name)
            {
                var renameParameters = new Dictionary<string, object>
                {
                    ["VMSwitch"] = SelectedVirtualSwitch.Name,
                    ["NewName"] = EditingSwitchName
                };

                await Task.Run(() => _hyperVInstance.InvokeFunction<object>("Rename-VMSwitch", renameParameters));
            }

            // 更新备注
            var setParameters = new Dictionary<string, object>
            {
                ["Name"] = EditingSwitchName,
                ["Notes"] = EditingSwitchNotes
            };

            await Task.Run(() => _hyperVInstance.InvokeFunction<object>("Set-VMSwitch", setParameters));

            IsEditingDetails = false;
            StatusMessage = "更改保存成功";
            
            await LoadVirtualSwitchesAsync();
            
            // 重新选择已编辑的交换机
            SelectedVirtualSwitch = VirtualSwitches.FirstOrDefault(s => s.Name == EditingSwitchName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存更改失败: {ex.Message}";
            MessageBox.Show($"保存更改失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
