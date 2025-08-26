using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using Facet;
using Microsoft.HyperV.PowerShell;
using Wpf.Ui.Abstractions.Controls;
using FluentHyperV.HyperV;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using PowerShell = FluentHyperV.PowerShell;

namespace FluentHyperV.Desktop.ViewModels.Pages;

public partial class CreateVirtualMachineViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;
    private readonly HyperVApi _hyperVApi;

    [ObservableProperty]
    private string virtualMachineName = string.Empty;

    [ObservableProperty]
    private int generation = 2;

    [ObservableProperty]
    private long memoryStartupMB = 2048;

    [ObservableProperty]
    private bool dynamicMemoryEnabled = true;

    [ObservableProperty]
    private long minimumMemoryMB = 512;

    [ObservableProperty]
    private long maximumMemoryMB = 4096;

    [ObservableProperty]
    private int processorCount = 2;

    [ObservableProperty]
    private bool createVirtualHardDisk = true;

    [ObservableProperty]
    private string virtualHardDiskPath = string.Empty;

    [ObservableProperty]
    private long virtualHardDiskSizeGB = 40;

    [ObservableProperty]
    private string existingVhdPath = string.Empty;

    [ObservableProperty]
    private bool useExistingVhd = false;

    [ObservableProperty]
    private ObservableCollection<VMSwitch> availableSwitches = new();

    [ObservableProperty]
    private VMSwitch? selectedSwitch;

    [ObservableProperty]
    private bool connectToNetwork = true;

    [ObservableProperty]
    private string vmPath = string.Empty;

    [ObservableProperty]
    private bool isCreating = false;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool enableSecureBoot = true;

    [ObservableProperty]
    private string bootDevice = "VHD";

    [ObservableProperty]
    private int currentStep = 1;

    [ObservableProperty]
    private bool canGoPrevious = false;

    [ObservableProperty]
    private bool canGoNext = true;

    public ObservableCollection<string> GenerationOptions { get; } = new() { "1", "2" };
    
    public ObservableCollection<string> BootDeviceOptions { get; } = new() 
    { 
        "VHD", "CD", "NetworkAdapter", "Floppy", "IDE", "LegacyNetworkAdapter" 
    };

    public CreateVirtualMachineViewModel()
    {
        _hyperVApi = new HyperVApi();
        
        // 设置默认路径
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var defaultVmPath = Path.Combine(userProfile, "Documents", "Virtual Machines");
        VmPath = defaultVmPath;
        
        VirtualHardDiskPath = Path.Combine(defaultVmPath, "Virtual Hard Disks");
        
        // 初始化步骤导航状态
        UpdateNavigationState();
    }

    partial void OnVirtualMachineNameChanged(string value)
    {
        UpdateNavigationState();
        
        // 当虚拟机名称改变时，更新虚拟硬盘路径
        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(VirtualHardDiskPath))
        {
            var directory = Path.GetDirectoryName(VirtualHardDiskPath);
            if (!string.IsNullOrEmpty(directory))
            {
                VirtualHardDiskPath = Path.Combine(directory, $"{value}.vhdx");
            }
        }
    }

    partial void OnMemoryStartupMBChanged(long value)
    {
        UpdateNavigationState();
    }

    private void UpdateNavigationState()
    {
        CanGoPrevious = CurrentStep > 1;
        CanGoNext = CurrentStep < 8 && ValidateCurrentStep();
    }

    private bool ValidateCurrentStep()
    {
        return CurrentStep switch
        {
            1 => true, // 开始之前页面，无需验证
            2 => !string.IsNullOrWhiteSpace(VirtualMachineName), // 需要名称
            3 => true, // 代数选择，默认已选择
            4 => MemoryStartupMB >= 32, // 内存验证
            5 => true, // 网络配置，可选
            6 => true, // 虚拟硬盘，可选
            7 => true, // 安装选项，可选
            8 => true, // 摘要页面
            _ => false
        };
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CanGoNext && CurrentStep < 8)
        {
            CurrentStep++;
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CanGoPrevious && CurrentStep > 1)
        {
            CurrentStep--;
        }
    }

    public async Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
        {
            await LoadVMSwitchesAsync();
            _isInitialized = true;
        }
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    [RelayCommand]
    private async Task LoadVMSwitchesAsync()
    {
        try
        {
            StatusMessage = "正在加载虚拟交换机...";
            
            var switches = await _hyperVApi.GetVMSwitchAsync(new HyperVApi.GetVMSwitchArguments());
            
            AvailableSwitches.Clear();
            foreach (var vmSwitch in switches)
            {
                AvailableSwitches.Add(vmSwitch);
            }

            // 默认选择第一个交换机
            if (AvailableSwitches.Count > 0)
            {
                SelectedSwitch = AvailableSwitches.First();
            }

            StatusMessage = $"已加载 {AvailableSwitches.Count} 个虚拟交换机";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载虚拟交换机失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void BrowseVMPath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog()
        {
            Title = "选择虚拟机存储路径",
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "选择文件夹"
        };

        if (dialog.ShowDialog() == true)
        {
            VmPath = Path.GetDirectoryName(dialog.FileName) ?? VmPath;
        }
    }

    [RelayCommand]
    private void BrowseVHDPath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog()
        {
            Title = "选择虚拟硬盘路径",
            Filter = "Virtual Hard Disk files (*.vhdx)|*.vhdx|Virtual Hard Disk files (*.vhd)|*.vhd",
            DefaultExt = ".vhdx",
            FileName = $"{VirtualMachineName}.vhdx"
        };

        if (dialog.ShowDialog() == true)
        {
            VirtualHardDiskPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void BrowseExistingVHD()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog()
        {
            Title = "选择现有虚拟硬盘",
            Filter = "Virtual Hard Disk files (*.vhdx;*.vhd)|*.vhdx;*.vhd|VHDX files (*.vhdx)|*.vhdx|VHD files (*.vhd)|*.vhd",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            ExistingVhdPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task CreateVirtualMachineAsync()
    {
        if (!ValidateInput())
            return;

        try
        {
            IsCreating = true;
            StatusMessage = "正在创建虚拟机...";

            var args = new HyperVApi.NewVMArguments
            {
                Name = VirtualMachineName,
                Generation = (short)Generation,
                MemoryStartupBytes = MemoryStartupMB * 1024 * 1024, // 转换为字节
                Path = VmPath,
                NewVHDPath = null,
                NewVHDSizeBytes = null,
                VHDPath = null
            };

            // 设置虚拟硬盘
            if (!UseExistingVhd && CreateVirtualHardDisk)
            {
                args.NewVHDPath = Path.Combine(VirtualHardDiskPath, $"{VirtualMachineName}.vhdx");
                args.NewVHDSizeBytes = (ulong)(VirtualHardDiskSizeGB * 1024 * 1024 * 1024); // 转换为字节
            }
            else if (UseExistingVhd && !string.IsNullOrEmpty(ExistingVhdPath))
            {
                args.VHDPath = ExistingVhdPath;
            }
            else
            {
                args.NoVHD = true; // 不创建虚拟硬盘
            }

            // 设置网络交换机
            if (ConnectToNetwork && SelectedSwitch != null)
            {
                args.SwitchName = SelectedSwitch.Name;
            }

            // 设置引导设备
            if (Enum.TryParse<BootDevice>(BootDevice, out var bootDeviceEnum))
            {
                args.BootDevice = bootDeviceEnum;
            }

            StatusMessage = "正在执行创建命令...";
            var result = await _hyperVApi.NewVMAsync(args);

            if (result != null && result.Length > 0)
            {
                var createdVM = result[0];
                StatusMessage = $"虚拟机 '{createdVM.Name}' 创建成功！";

                // 如果启用了动态内存，配置内存设置
                if (DynamicMemoryEnabled && Generation == 2)
                {
                    try
                    {
                        StatusMessage = "正在配置动态内存...";
                        await ConfigureDynamicMemoryAsync(createdVM.Name);
                    }
                    catch (Exception ex)
                    {
                        StatusMessage += $" 但动态内存配置失败: {ex.Message}";
                    }
                }

                // 配置处理器数量
                if (ProcessorCount > 1)
                {
                    try
                    {
                        StatusMessage = "正在配置处理器...";
                        await ConfigureProcessorAsync(createdVM.Name);
                    }
                    catch (Exception ex)
                    {
                        StatusMessage += $" 但处理器配置失败: {ex.Message}";
                    }
                }

                // 重置表单
                ResetForm();
                
                MessageBox.Show($"虚拟机 '{createdVM.Name}' 创建成功！", "创建成功", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "创建虚拟机失败：未返回创建结果";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"创建虚拟机失败: {ex.Message}";
            MessageBox.Show($"创建虚拟机失败:\n{ex.Message}", "错误", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsCreating = false;
        }
    }

    private async Task ConfigureDynamicMemoryAsync(string vmName)
    {
        // 这里需要调用Set-VMMemory命令来配置动态内存
        // 由于当前的HyperVApi.g.cs可能不包含这个方法，我们可以使用PowerShell实例直接执行
        await Task.Run(() =>
        {
            using var psInstance = new PowerShell.PowerShellInstance();
            
            var script = $@"
                Set-VMMemory -VMName '{vmName}' -DynamicMemoryEnabled $true -MinimumBytes {MinimumMemoryMB * 1024 * 1024} -MaximumBytes {MaximumMemoryMB * 1024 * 1024}
            ";
            
            psInstance.ExecuteScript(script);
        });
    }

    private async Task ConfigureProcessorAsync(string vmName)
    {
        // 配置处理器数量
        await Task.Run(() =>
        {
            using var psInstance = new PowerShell.PowerShellInstance();
            
            var script = $@"
                Set-VMProcessor -VMName '{vmName}' -Count {ProcessorCount}
            ";
            
            psInstance.ExecuteScript(script);
        });
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(VirtualMachineName))
        {
            StatusMessage = "请输入虚拟机名称";
            return false;
        }

        if (VirtualMachineName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            StatusMessage = "虚拟机名称包含无效字符";
            return false;
        }

        if (MemoryStartupMB < 32)
        {
            StatusMessage = "启动内存不能少于32MB";
            return false;
        }

        if (DynamicMemoryEnabled)
        {
            if (MinimumMemoryMB > MemoryStartupMB)
            {
                StatusMessage = "最小内存不能大于启动内存";
                return false;
            }

            if (MaximumMemoryMB < MemoryStartupMB)
            {
                StatusMessage = "最大内存不能小于启动内存";
                return false;
            }
        }

        if (ProcessorCount < 1 || ProcessorCount > 64)
        {
            StatusMessage = "处理器数量必须在1-64之间";
            return false;
        }

        if (CreateVirtualHardDisk && !UseExistingVhd)
        {
            if (VirtualHardDiskSizeGB < 1)
            {
                StatusMessage = "虚拟硬盘大小至少为1GB";
                return false;
            }

            if (string.IsNullOrWhiteSpace(VirtualHardDiskPath))
            {
                StatusMessage = "请指定虚拟硬盘路径";
                return false;
            }
        }

        if (UseExistingVhd && string.IsNullOrWhiteSpace(ExistingVhdPath))
        {
            StatusMessage = "请选择现有的虚拟硬盘文件";
            return false;
        }

        if (ConnectToNetwork && SelectedSwitch == null)
        {
            StatusMessage = "请选择一个虚拟交换机";
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void ResetForm()
    {
        VirtualMachineName = string.Empty;
        Generation = 2;
        MemoryStartupMB = 2048;
        DynamicMemoryEnabled = true;
        MinimumMemoryMB = 512;
        MaximumMemoryMB = 4096;
        ProcessorCount = 2;
        CreateVirtualHardDisk = true;
        VirtualHardDiskSizeGB = 40;
        UseExistingVhd = false;
        ExistingVhdPath = string.Empty;
        ConnectToNetwork = true;
        EnableSecureBoot = true;
        BootDevice = "VHD";
        StatusMessage = string.Empty;
        
        // 重新设置虚拟硬盘路径
        if (!string.IsNullOrEmpty(VmPath))
        {
            VirtualHardDiskPath = Path.Combine(VmPath, "Virtual Hard Disks");
        }
    }

    partial void OnUseExistingVhdChanged(bool value)
    {
        if (value)
        {
            CreateVirtualHardDisk = false;
        }
    }

    partial void OnCreateVirtualHardDiskChanged(bool value)
    {
        if (value)
        {
            UseExistingVhd = false;
        }
    }

    partial void OnGenerationChanged(int value)
    {
        // Generation 1 VM的一些限制
        if (value == 1)
        {
            EnableSecureBoot = false;
            if (BootDevice == "NetworkAdapter")
            {
                BootDevice = "LegacyNetworkAdapter";
            }
        }
        else
        {
            EnableSecureBoot = true;
            if (BootDevice == "LegacyNetworkAdapter")
            {
                BootDevice = "NetworkAdapter";
            }
        }
    }
}
