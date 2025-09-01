using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Diagnostics;

namespace GamingTweaksManager.Services
{
    /// <summary>
    /// Serviço responsável por detectar hardware do sistema para filtrar tweaks compatíveis
    /// </summary>
    public class HardwareDetectionService
    {
        private HardwareInfo _hardwareInfo;

        public string GpuVendor => _hardwareInfo?.GpuVendor ?? "Unknown";
        public string CpuVendor => _hardwareInfo?.CpuVendor ?? "Unknown";
        public string CpuInfo => _hardwareInfo?.CpuName ?? "Unknown";
        public string GpuInfo => string.Join(", ", _hardwareInfo?.GpuNames ?? new List<string>());
        public string RamInfo => _hardwareInfo?.RamInfo ?? "Unknown";
        public string RamFrequency => _hardwareInfo?.RamFrequency ?? "Unknown";
        public string Motherboard => _hardwareInfo?.Motherboard ?? "Unknown";
        public string Storage => _hardwareInfo?.Storage ?? "Unknown";
        public string CpuCores => _hardwareInfo?.CpuCores ?? "Unknown";
        public string CpuFrequency => _hardwareInfo?.CpuFrequency ?? "Unknown";

        public HardwareDetectionService()
        {
            _hardwareInfo = DetectHardware();
        }

        /// <summary>
        /// Obtém informações completas de hardware para exibição
        /// </summary>
        public HardwareInfo GetDetailedHardwareInfo()
        {
            return _hardwareInfo;
        }

        /// <summary>
        /// Verifica se o hardware é compatível com o tweak
        /// </summary>
        public bool IsCompatible(string hardwareType)
        {
            return hardwareType switch
            {
                "NVIDIA" => GpuVendor.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase),
                "AMD" => GpuVendor.Contains("AMD", StringComparison.OrdinalIgnoreCase),
                "Intel" => CpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase) || 
                          GpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase),
                "Universal" => true,
                _ => true
            };
        }

        /// <summary>
        /// Obtém informações detalhadas do hardware em formato de texto
        /// </summary>
        public string GetHardwareInfo()
        {
            if (_hardwareInfo == null)
                _hardwareInfo = DetectHardware();

            var sb = new StringBuilder();
            sb.AppendLine($"🖥️ Processador: {_hardwareInfo.CpuName}");
            sb.AppendLine($"   • Núcleos: {_hardwareInfo.CpuCores} | Frequência: {_hardwareInfo.CpuFrequency}");
            sb.AppendLine($"   • Uso atual: {GetCpuUsage():F1}%");
            sb.AppendLine();
            sb.AppendLine($"🎮 Placa de Vídeo: {string.Join(", ", _hardwareInfo.GpuNames)}");
            sb.AppendLine();
            sb.AppendLine($"💾 Memória RAM: {_hardwareInfo.RamInfo}");
            sb.AppendLine($"   • Frequência: {_hardwareInfo.RamFrequency}");
            sb.AppendLine($"   • Uso atual: {GetMemoryUsage():F1}% ({GetUsedMemory():F1} GB em uso)");
            sb.AppendLine();
            sb.AppendLine($"🔧 Placa-mãe: {_hardwareInfo.Motherboard}");
            sb.AppendLine();
            sb.AppendLine($"💿 Armazenamento: {_hardwareInfo.Storage}");
            sb.AppendLine($"   • Espaço livre: {GetDiskFreeSpace():F1} GB");

            return sb.ToString();
        }

        /// <summary>
        /// Obtém o uso atual da CPU
        /// </summary>
        private double GetCpuUsage()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var usage = obj["LoadPercentage"]?.ToString();
                        if (double.TryParse(usage, out double cpuUsage))
                            return cpuUsage;
                    }
                }
            }
            catch
            {
                // Fallback usando PerformanceCounter
                try
                {
                    using (var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                    {
                        pc.NextValue();
                        System.Threading.Thread.Sleep(100);
                        return pc.NextValue();
                    }
                }
                catch { }
            }
            return 0;
        }

        /// <summary>
        /// Obtém o uso atual da memória
        /// </summary>
        private double GetMemoryUsage()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, AvailablePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var total = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                        var available = Convert.ToDouble(obj["AvailablePhysicalMemory"]);
                        var used = total - available;
                        return (used / total) * 100;
                    }
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Obtém a memória usada em GB
        /// </summary>
        private double GetUsedMemory()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, AvailablePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var total = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                        var available = Convert.ToDouble(obj["AvailablePhysicalMemory"]);
                        var used = total - available;
                        return (used * 1024) / (1024 * 1024 * 1024); // Converter KB para GB
                    }
                }
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// Obtém o espaço livre em disco
        /// </summary>
        private double GetDiskFreeSpace()
        {
            try
            {
                var drive = new System.IO.DriveInfo("C:");
                return drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0); // Bytes para GB
            }
            catch { }
            return 0;
        }
        public class HardwareInfo
        {
            public string GpuVendor { get; set; } = "Unknown";
            public string CpuVendor { get; set; } = "Unknown";
            public List<string> GpuNames { get; set; } = new List<string>();
            public string CpuName { get; set; } = "Unknown";
            public string RamInfo { get; set; } = "Unknown";
            public string RamFrequency { get; set; } = "Unknown";
            public string Motherboard { get; set; } = "Unknown";
            public string Storage { get; set; } = "Unknown";
            public string CpuCores { get; set; } = "Unknown";
            public string CpuFrequency { get; set; } = "Unknown";
            public bool HasNvidia => GpuVendor.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase);
            public bool HasAmd => GpuVendor.Contains("AMD", StringComparison.OrdinalIgnoreCase) || 
                                  GpuVendor.Contains("ATI", StringComparison.OrdinalIgnoreCase);
            public bool HasIntel => GpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase);
            public bool HasIntelCpu => CpuVendor.Contains("Intel", StringComparison.OrdinalIgnoreCase);
            public bool HasAmdCpu => CpuVendor.Contains("AMD", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Detecta informações de hardware do sistema
        /// </summary>
        /// <returns>Informações de hardware detectadas</returns>
        public static HardwareInfo DetectHardware()
        {
            var hardwareInfo = new HardwareInfo();

            try
            {
                // Detectar GPU
                DetectGpu(hardwareInfo);
                
                // Detectar CPU
                DetectCpu(hardwareInfo);
                
                // Detectar RAM
                DetectRam(hardwareInfo);
                
                // Detectar Motherboard
                DetectMotherboard(hardwareInfo);
                
                // Detectar Storage
                DetectStorage(hardwareInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar hardware: {ex.Message}");
            }

            return hardwareInfo;
        }

        private static void DetectGpu(HardwareInfo hardwareInfo)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    var gpuVendors = new List<string>();
                    
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            hardwareInfo.GpuNames.Add(name);
                            
                            if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
                                gpuVendors.Add("NVIDIA");
                            else if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase) || 
                                     name.Contains("ATI", StringComparison.OrdinalIgnoreCase))
                                gpuVendors.Add("AMD");
                            else if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase))
                                gpuVendors.Add("Intel");
                        }
                    }

                    if (gpuVendors.Any())
                        hardwareInfo.GpuVendor = string.Join(", ", gpuVendors.Distinct());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar GPU: {ex.Message}");
            }
        }

        private static void DetectCpu(HardwareInfo hardwareInfo)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString();
                        var cores = obj["NumberOfCores"]?.ToString();
                        var frequency = obj["MaxClockSpeed"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(name))
                        {
                            hardwareInfo.CpuName = name.Trim();
                            hardwareInfo.CpuCores = cores ?? "Unknown";
                            
                            if (!string.IsNullOrEmpty(frequency))
                            {
                                if (double.TryParse(frequency, out double freq))
                                {
                                    hardwareInfo.CpuFrequency = $"{freq / 1000:F1} GHz";
                                }
                            }
                            
                            if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase))
                                hardwareInfo.CpuVendor = "Intel";
                            else if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase))
                                hardwareInfo.CpuVendor = "AMD";
                            
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar CPU: {ex.Message}");
            }
        }

        private static void DetectRam(HardwareInfo hardwareInfo)
        {
            try
            {
                long totalMemory = 0;
                var frequencies = new List<int>();
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var capacity = obj["Capacity"]?.ToString();
                        var speed = obj["Speed"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(capacity) && long.TryParse(capacity, out long mem))
                        {
                            totalMemory += mem;
                        }
                        
                        if (!string.IsNullOrEmpty(speed) && int.TryParse(speed, out int freq))
                        {
                            frequencies.Add(freq);
                        }
                    }
                }
                
                if (totalMemory > 0)
                {
                    hardwareInfo.RamInfo = $"{totalMemory / (1024 * 1024 * 1024)} GB";
                }
                
                if (frequencies.Any())
                {
                    hardwareInfo.RamFrequency = $"{frequencies.Max()} MHz";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar RAM: {ex.Message}");
            }
        }

        private static void DetectMotherboard(HardwareInfo hardwareInfo)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var manufacturer = obj["Manufacturer"]?.ToString();
                        var product = obj["Product"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(product))
                        {
                            hardwareInfo.Motherboard = $"{manufacturer} {product}";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar Motherboard: {ex.Message}");
            }
        }

        private static void DetectStorage(HardwareInfo hardwareInfo)
        {
            try
            {
                var storageDevices = new List<string>();
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var model = obj["Model"]?.ToString();
                        var size = obj["Size"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(size))
                        {
                            if (long.TryParse(size, out long sizeBytes))
                            {
                                var sizeGB = sizeBytes / (1024 * 1024 * 1024);
                                storageDevices.Add($"{model} ({sizeGB} GB)");
                            }
                        }
                    }
                }
                
                if (storageDevices.Any())
                {
                    hardwareInfo.Storage = string.Join(", ", storageDevices.Take(2));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao detectar Storage: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se um tweak é compatível com o hardware atual
        /// </summary>
        /// <param name="tweakCompatibility">String de compatibilidade do tweak</param>
        /// <param name="hardwareInfo">Informações de hardware</param>
        /// <returns>True se compatível</returns>
        public static bool IsCompatible(string tweakCompatibility, HardwareInfo hardwareInfo)
        {
            if (string.IsNullOrEmpty(tweakCompatibility) || tweakCompatibility.Equals("Universal", StringComparison.OrdinalIgnoreCase))
                return true;

            var compatibility = tweakCompatibility.ToLowerInvariant();

            if (compatibility.Contains("nvidia") && !hardwareInfo.HasNvidia)
                return false;
            
            if (compatibility.Contains("amd") && !hardwareInfo.HasAmd && !hardwareInfo.HasAmdCpu)
                return false;
            
            if (compatibility.Contains("intel") && !hardwareInfo.HasIntel && !hardwareInfo.HasIntelCpu)
                return false;

            return true;
        }
    }
}
