using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Facet;
using FluentHyperV.HyperV;
using Microsoft.HyperV.PowerShell;
using Wpf.Ui.Abstractions.Controls;
using PowerShell = FluentHyperV.PowerShell;

namespace FluentHyperV.Desktop.ViewModels.Pages;

public partial class CreateVirtualMachineViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;
    private readonly HyperVApi _hyperVApi;

    [ObservableProperty]
    private string _virtualMachineName = string.Empty;

    [ObservableProperty]
    private int _generation = 2;

    [ObservableProperty]
    private long _memoryStartupMb = 2048;

    [ObservableProperty]
    private bool _dynamicMemoryEnabled = true;

    [ObservableProperty]
    private long _minimumMemoryMb = 512;

    [ObservableProperty]
    private long _maximumMemoryMb = 4096;

    [ObservableProperty]
    private int _processorCount = 2;

    [ObservableProperty]
    private bool _createVirtualHardDisk = true;

    [ObservableProperty]
    private string _virtualHardDiskPath = string.Empty;

    [ObservableProperty]
    private long _virtualHardDiskSizeGb = 40;

    [ObservableProperty]
    private string _existingVhdPath = string.Empty;

    [ObservableProperty]
    private bool _useExistingVhd = false;

    [ObservableProperty]
    private ObservableCollection<VMSwitch> _availableSwitches = new();

    [ObservableProperty]
    private VMSwitch? _selectedSwitch;

    [ObservableProperty]
    private bool _connectToNetwork = true;

    [ObservableProperty]
    private string _vmPath = string.Empty;

    [ObservableProperty]
    private bool _isCreating = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _enableSecureBoot = true;

    [ObservableProperty]
    private string _bootDevice = "VHD";

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private bool _canGoPrevious = false;

    [ObservableProperty]
    private bool _canGoNext = true;

    [ObservableProperty]
    private int _maxCompletedStep = 1;

    [ObservableProperty]
    private bool _useCustomLocation;

    [ObservableProperty]
    private bool _skipVhdConfiguration;

    [ObservableProperty]
    private string _isoPath = string.Empty;

    public bool CanFinish => CurrentStep == 8 && !string.IsNullOrWhiteSpace(VirtualMachineName);

    public ObservableCollection<string> GenerationOptions { get; } = new() { "1", "2" };

    public ObservableCollection<string> BootDeviceOptions { get; } =
        new() { "VHD", "CD", "NetworkAdapter", "Floppy", "IDE", "LegacyNetworkAdapter" };

    public CreateVirtualMachineViewModel()
    {
        _hyperVApi = new HyperVApi();

        // 设置默认路径
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var defaultVmPath = Path.Combine(userProfile, "Documents", "Virtual Machines");
        VmPath = defaultVmPath;

        VirtualHardDiskPath = Path.Combine(defaultVmPath, "Virtual Hard Disks");

        // 初始化步骤导航状态
        MaxCompletedStep = 0; // 开始时允许访问欢迎页(索引0)和第一步(索引1)
        System.Diagnostics.Debug.WriteLine($"ViewModel初始化: CurrentStep={CurrentStep}, MaxCompletedStep={MaxCompletedStep}");
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

    partial void OnMemoryStartupMbChanged(long value)
    {
        UpdateNavigationState();
    }

    partial void OnDynamicMemoryEnabledChanged(bool value)
    {
        UpdateNavigationState();
    }

    partial void OnMinimumMemoryMbChanged(long value)
    {
        UpdateNavigationState();
    }

    partial void OnMaximumMemoryMbChanged(long value)
    {
        UpdateNavigationState();
    }

    partial void OnVirtualHardDiskSizeGbChanged(long value)
    {
        UpdateNavigationState();
    }

    partial void OnVirtualHardDiskPathChanged(string value)
    {
        UpdateNavigationState();
    }

    partial void OnExistingVhdPathChanged(string value)
    {
        UpdateNavigationState();
    }

    partial void OnSkipVhdConfigurationChanged(bool value)
    {
        UpdateNavigationState();
    }

    partial void OnCurrentStepChanged(int value)
    {
        UpdateNavigationState();
        OnPropertyChanged(nameof(CanFinish));
    }

    private void UpdateNavigationState()
    {
        var previousMaxStep = MaxCompletedStep;
        CanGoPrevious = CurrentStep > 1; // 只有当前步骤大于1时才能回退
        CanGoNext = CurrentStep < 8 && ValidateCurrentStep();

        // 如果当前步骤验证通过，更新最大完成步骤
        if (ValidateCurrentStep())
        {
            MaxCompletedStep = Math.Max(MaxCompletedStep, CurrentStep + 1);
        }

        if (MaxCompletedStep != previousMaxStep)
        {
            System.Diagnostics.Debug.WriteLine($"MaxCompletedStep更新: {previousMaxStep} -> {MaxCompletedStep} (当前步骤: {CurrentStep})");
        }
    }

    private bool ValidateCurrentStep()
    {
        return CurrentStep switch
        {
            0 => true, // 欢迎页面，无需验证
            1 => true, // 开始之前页面，无需验证
            2 => !string.IsNullOrWhiteSpace(VirtualMachineName) && VirtualMachineName.Trim().Length > 0, // 需要有效名称
            3 => Generation == 1 || Generation == 2, // 代数选择验证
            4 => MemoryStartupMb >= 32 && MemoryStartupMb <= 1048576 &&
                 (!DynamicMemoryEnabled || (MinimumMemoryMb >= 32 && MaximumMemoryMb >= MemoryStartupMb)), // 内存验证
            5 => true, // 网络配置，可选
            6 => ValidateVhdConfiguration(), // 虚拟硬盘配置验证
            7 => true, // 安装选项，可选
            8 => true, // 摘要页面
            _ => false,
        };
    }

    private bool ValidateVhdConfiguration()
    {
        if (SkipVhdConfiguration)
            return true;

        if (CreateVirtualHardDisk && !UseExistingVhd)
        {
            return VirtualHardDiskSizeGb >= 1 && VirtualHardDiskSizeGb <= 64000 &&
                   !string.IsNullOrWhiteSpace(VirtualHardDiskPath);
        }

        if (UseExistingVhd)
        {
            return !string.IsNullOrWhiteSpace(ExistingVhdPath) && File.Exists(ExistingVhdPath);
        }

        return true;
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CanGoNext && CurrentStep < 8)
        {
            CurrentStep++;
            // 更新最大完成步骤
            if (CurrentStep > MaxCompletedStep)
            {
                MaxCompletedStep = CurrentStep;
            }
            UpdateNavigationState();
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CanGoPrevious && CurrentStep > 1) // 防止回退到欢迎页面(索引0)
        {
            CurrentStep--;
            UpdateNavigationState();
        }
    }

    [RelayCommand]
    private void GoToStep(object parameter)
    {
        if (parameter is int step)
        {
            // 只允许跳转到已完成的步骤或下一步
            if (step <= MaxCompletedStep || step == MaxCompletedStep + 1)
            {
                CurrentStep = step;
                UpdateNavigationState();
            }
        }
    }

    public bool CanAccessStep(int step)
    {
        return step <= MaxCompletedStep || step == MaxCompletedStep + 1;
    }

    private void ResetWizardState()
    {
        // 重置所有向导状态
        CurrentStep = 1;
        MaxCompletedStep = 1;
        CanGoPrevious = false;
        CanGoNext = true;

        // 重置所有表单字段
        VirtualMachineName = string.Empty;
        Generation = 1; // 默认第1代
        MemoryStartupMb = 2048;
        DynamicMemoryEnabled = true;
        MinimumMemoryMb = 512;
        MaximumMemoryMb = 4096;
        ProcessorCount = 2;
        CreateVirtualHardDisk = true;
        UseExistingVhd = false;
        SkipVhdConfiguration = false;
        ExistingVhdPath = string.Empty;
        ConnectToNetwork = true;
        UseCustomLocation = false;
        IsoPath = string.Empty;
        BootDevice = "VHD";
        IsCreating = false;
        StatusMessage = string.Empty;

        // 重置路径
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var defaultVmPath = Path.Combine(userProfile, "Documents", "Virtual Machines");
        VmPath = defaultVmPath;
        VirtualHardDiskPath = Path.Combine(defaultVmPath, "Virtual Hard Disks");

        // 清除选中的交换机
        SelectedSwitch = AvailableSwitches.FirstOrDefault();

        System.Diagnostics.Debug.WriteLine("向导状态已重置");
    }

    public async Task OnNavigatedToAsync()
    {
        // 每次导航到页面时重置所有状态
        ResetWizardState();

        if (!_isInitialized)
        {
            await LoadVmSwitchesAsync();
            _isInitialized = true;
        }
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    [RelayCommand]
    private async Task LoadVmSwitchesAsync()
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
    private void BrowseVmPath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog()
        {
            Title = "选择虚拟机存储路径",
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "选择文件夹",
        };

        if (dialog.ShowDialog() == true)
        {
            VmPath = Path.GetDirectoryName(dialog.FileName) ?? VmPath;
        }
    }

    [RelayCommand]
    private void BrowseVhdPath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog()
        {
            Title = "选择虚拟硬盘路径",
            Filter =
                "Virtual Hard Disk files (*.vhdx)|*.vhdx|Virtual Hard Disk files (*.vhd)|*.vhd",
            DefaultExt = ".vhdx",
            FileName = $"{VirtualMachineName}.vhdx",
        };

        if (dialog.ShowDialog() == true)
        {
            VirtualHardDiskPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void BrowseExistingVhd()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog()
        {
            Title = "选择现有虚拟硬盘",
            Filter =
                "Virtual Hard Disk files (*.vhdx;*.vhd)|*.vhdx;*.vhd|VHDX files (*.vhdx)|*.vhdx|VHD files (*.vhd)|*.vhd",
            CheckFileExists = true,
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
                MemoryStartupBytes = MemoryStartupMb * 1024 * 1024, // 转换为字节
                Path = VmPath,
                NewVHDPath = null,
                NewVHDSizeBytes = null,
                VHDPath = null,
            };

            // 设置虚拟硬盘
            if (!UseExistingVhd && CreateVirtualHardDisk)
            {
                args.NewVHDPath = Path.Combine(VirtualHardDiskPath, $"{VirtualMachineName}.vhdx");
                args.NewVHDSizeBytes = (ulong)(VirtualHardDiskSizeGb * 1024 * 1024 * 1024); // 转换为字节
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
                var createdVm = result[0];
                StatusMessage = $"虚拟机 '{createdVm.Name}' 创建成功！";

                // 如果启用了动态内存，配置内存设置
                if (DynamicMemoryEnabled && Generation == 2)
                {
                    try
                    {
                        StatusMessage = "正在配置动态内存...";
                        await ConfigureDynamicMemoryAsync(createdVm.Name);
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
                        await ConfigureProcessorAsync(createdVm.Name);
                    }
                    catch (Exception ex)
                    {
                        StatusMessage += $" 但处理器配置失败: {ex.Message}";
                    }
                }

                // 重置表单
                ResetForm();

                MessageBox.Show(
                    $"虚拟机 '{createdVm.Name}' 创建成功！",
                    "创建成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                StatusMessage = "创建虚拟机失败：未返回创建结果";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"创建虚拟机失败: {ex.Message}";
            MessageBox.Show(
                $"创建虚拟机失败:\n{ex.Message}",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
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

            var script =
                $@"
                Set-VMMemory -VMName '{vmName}' -DynamicMemoryEnabled $true -MinimumBytes {MinimumMemoryMb * 1024 * 1024} -MaximumBytes {MaximumMemoryMb * 1024 * 1024}
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

            var script =
                $@"
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

        if (MemoryStartupMb < 32)
        {
            StatusMessage = "启动内存不能少于32MB";
            return false;
        }

        if (DynamicMemoryEnabled)
        {
            if (MinimumMemoryMb > MemoryStartupMb)
            {
                StatusMessage = "最小内存不能大于启动内存";
                return false;
            }

            if (MaximumMemoryMb < MemoryStartupMb)
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
            if (VirtualHardDiskSizeGb < 1)
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
        MemoryStartupMb = 2048;
        DynamicMemoryEnabled = true;
        MinimumMemoryMb = 512;
        MaximumMemoryMb = 4096;
        ProcessorCount = 2;
        CreateVirtualHardDisk = true;
        VirtualHardDiskSizeGb = 40;
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
        UpdateNavigationState();
    }

    partial void OnCreateVirtualHardDiskChanged(bool value)
    {
        if (value)
        {
            UseExistingVhd = false;
        }
        UpdateNavigationState();
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
        UpdateNavigationState();
    }
}
