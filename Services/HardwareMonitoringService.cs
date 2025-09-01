using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço para monitoramento em tempo real de hardware (CPU, GPU, temperatura, uso)
    /// </summary>
    public class HardwareMonitoringService : INotifyPropertyChanged, IDisposable
    {
        private readonly Timer _monitoringTimer;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        
        private string _cpuModel;
        private string _gpuModel;
        private double _cpuUsage;
        private double _ramUsage;
        private double _cpuTemperature;
        private double _gpuTemperature;
        private double _gpuUsage;
        private string _cpuFrequency;
        private long _totalRam;
        private long _availableRam;

        public string CpuModel
        {
            get => _cpuModel;
            set { _cpuModel = value; OnPropertyChanged(); }
        }

        public string GpuModel
        {
            get => _gpuModel;
            set { _gpuModel = value; OnPropertyChanged(); }
        }

        public double CpuUsage
        {
            get => _cpuUsage;
            set { _cpuUsage = value; OnPropertyChanged(); }
        }

        public double RamUsage
        {
            get => _ramUsage;
            set { _ramUsage = value; OnPropertyChanged(); }
        }

        public double CpuTemperature
        {
            get => _cpuTemperature;
            set { _cpuTemperature = value; OnPropertyChanged(); }
        }

        public double GpuTemperature
        {
            get => _gpuTemperature;
            set { _gpuTemperature = value; OnPropertyChanged(); }
        }

        public double GpuUsage
        {
            get => _gpuUsage;
            set { _gpuUsage = value; OnPropertyChanged(); }
        }

        public string CpuFrequency
        {
            get => _cpuFrequency;
            set { _cpuFrequency = value; OnPropertyChanged(); }
        }

        public long TotalRam
        {
            get => _totalRam;
            set { _totalRam = value; OnPropertyChanged(); }
        }

        public long AvailableRam
        {
            get => _availableRam;
            set { _availableRam = value; OnPropertyChanged(); }
        }

        public string RamUsageText => $"{(TotalRam - AvailableRam) / 1024 / 1024 / 1024:F1} GB / {TotalRam / 1024 / 1024 / 1024:F1} GB";
        public string CpuUsageText => $"{CpuUsage:F1}%";
        public string GpuUsageText => $"{GpuUsage:F1}%";
        public string CpuTempText => CpuTemperature > 0 ? $"{CpuTemperature:F0}°C" : "N/A";
        public string GpuTempText => GpuTemperature > 0 ? $"{GpuTemperature:F0}°C" : "N/A";

        public HardwareMonitoringService()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                
                // Primeira leitura para inicializar os contadores
                _cpuCounter.NextValue();
                _ramCounter.NextValue();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao inicializar contadores de performance: {ex.Message}");
            }

            _monitoringTimer = new Timer(2000); // Atualizar a cada 2 segundos
            _monitoringTimer.Elapsed += OnMonitoringTimerElapsed;
            
            // Inicializar informações de hardware
            _ = Task.Run(InitializeHardwareInfoAsync);
        }

        /// <summary>
        /// Inicia o monitoramento em tempo real
        /// </summary>
        public void StartMonitoring()
        {
            _monitoringTimer.Start();
        }

        /// <summary>
        /// Para o monitoramento
        /// </summary>
        public void StopMonitoring()
        {
            _monitoringTimer.Stop();
        }

        /// <summary>
        /// Inicializa informações básicas do hardware
        /// </summary>
        private async Task InitializeHardwareInfoAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Obter informações do CPU
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            CpuModel = obj["Name"]?.ToString()?.Trim();
                            var maxClockSpeed = obj["MaxClockSpeed"];
                            if (maxClockSpeed != null)
                            {
                                var speedGHz = Convert.ToDouble(maxClockSpeed) / 1000;
                                CpuFrequency = $"{speedGHz:F1} GHz";
                            }
                            break;
                        }
                    }

                    // Obter informações da GPU
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var name = obj["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(name) && 
                                (name.Contains("NVIDIA") || name.Contains("AMD") || name.Contains("Intel")))
                            {
                                GpuModel = name;
                                break;
                            }
                        }
                    }

                    // Obter informações da RAM
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var totalPhysicalMemory = obj["TotalPhysicalMemory"];
                            if (totalPhysicalMemory != null)
                            {
                                TotalRam = Convert.ToInt64(totalPhysicalMemory);
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao obter informações de hardware: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Atualiza métricas de performance em tempo real
        /// </summary>
        private async void OnMonitoringTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Atualizar uso do CPU
                    if (_cpuCounter != null)
                    {
                        CpuUsage = _cpuCounter.NextValue();
                    }

                    // Atualizar uso da RAM
                    if (_ramCounter != null)
                    {
                        AvailableRam = (long)_ramCounter.NextValue() * 1024 * 1024; // Converter MB para bytes
                        if (TotalRam > 0)
                        {
                            RamUsage = ((double)(TotalRam - AvailableRam) / TotalRam) * 100;
                        }
                    }

                    // Tentar obter temperatura do CPU (WMI)
                    TryGetCpuTemperature();

                    // Tentar obter informações da GPU
                    TryGetGpuInfo();

                    // Notificar mudanças nas propriedades calculadas
                    OnPropertyChanged(nameof(RamUsageText));
                    OnPropertyChanged(nameof(CpuUsageText));
                    OnPropertyChanged(nameof(GpuUsageText));
                    OnPropertyChanged(nameof(CpuTempText));
                    OnPropertyChanged(nameof(GpuTempText));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao atualizar métricas: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Tenta obter temperatura do CPU via WMI
        /// </summary>
        private void TryGetCpuTemperature()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var temp = obj["CurrentTemperature"];
                        if (temp != null)
                        {
                            // Converter de décimos de Kelvin para Celsius
                            var kelvin = Convert.ToDouble(temp) / 10.0;
                            CpuTemperature = kelvin - 273.15;
                            break;
                        }
                    }
                }
            }
            catch
            {
                // Temperatura não disponível via WMI
                CpuTemperature = 0;
            }
        }

        /// <summary>
        /// Tenta obter informações da GPU
        /// </summary>
        private void TryGetGpuInfo()
        {
            try
            {
                // Tentar obter uso da GPU via performance counters (limitado)
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_GPUPerformanceCounters_GPUEngine"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // Esta abordagem é limitada e pode não funcionar em todos os sistemas
                        var utilization = obj["UtilizationPercentage"];
                        if (utilization != null)
                        {
                            GpuUsage = Convert.ToDouble(utilization);
                            break;
                        }
                    }
                }
            }
            catch
            {
                // GPU usage não disponível via WMI padrão
                GpuUsage = 0;
            }
        }

        public void Dispose()
        {
            _monitoringTimer?.Stop();
            _monitoringTimer?.Dispose();
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
